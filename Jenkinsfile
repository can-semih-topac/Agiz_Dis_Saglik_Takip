// Jenkins'in çalıştıracağı pipeline — "Pipeline as Code": bu betik Jenkins arayüzüne
// gömülü değil, repo'nun bir parçası, yani Source Control'den takip edilebiliyor.
//
// "Selenium E2E Testi" adımı, localhost:5299'daki backend'in zaten ayakta olmasını
// bekliyor (canlı yığının parçası) — Jenkins onu başlatmıyor, sadece test için kullanıyor.
//
// "SonarQube Analizi"/"Kalite Kapısı Kontrolü" adımları da Selenium ile AYNI mantıkla
// bir güvenlik freni: SonarQube plugin'i Jenkins'e kurulmadığı için (sistem ayarlarına
// dokunmadan, sade bir dış araç gibi entegre edelim diye bilinçli tercih) "waitForQualityGate"
// pipeline adımı yok — bunun yerine SonarQube'ün Web API'sini kendimiz polluyoruz. Token,
// repo'nun dışında (C:\Users\canse\Jenkins\secrets\sonar-token.txt) duruyor, tıpkı .env gibi.
//
// Son adım Docker image'larını derleyip GERÇEK canlı portlarda (5299/4300, docker-compose.yml'in
// varsayılanları) ayağa kaldırıyor — yani her push, testler VE kalite kapısı geçtiği sürece,
// canlı siteyi otomatik günceller (dotnet watch/npx serve emekliye ayrıldı, canlı artık tamamen
// Docker container'larından oluşuyor). Herhangi bir adım başarısız olursa pipeline bir sonrakine
// hiç geçmez, canlı olduğu gibi kalır.
pipeline {
    agent any

    stages {
        stage('Backend Derle ve SonarQube Analizi') {
            steps {
                dir('backend') {
                    // "-o ci-build": canlıda sürekli çalışan "dotnet watch" kendi bin/Debug
                    // çıktısını sürekli kilitli tutuyor — Jenkins AYNI klasöre yazmaya
                    // çalışırsa her seferinde çakışır. Ayrı bir klasöre derleyerek bunu
                    // tamamen önlüyoruz, canlı sürece hiç dokunmuyoruz.
                    //
                    // SonarQube analizi "begin"/"end" arasında yapılan derlemeyi izliyor
                    // (Roslyn analizörlerini derlemeye enjekte ediyor) — bu yüzden derleme
                    // komutu ikisinin ARASINDA olmak zorunda, ayrı bir stage olamaz.
                    bat '''
                        for /f "usebackq delims=" %%t in ("C:\\Users\\canse\\Jenkins\\secrets\\sonar-token.txt") do set SONAR_TOKEN=%%t
                        dotnet-sonarscanner begin /k:"agiz-dis-saglik-takip" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="%SONAR_TOKEN%"
                        dotnet build -o ci-build
                        dotnet-sonarscanner end /d:sonar.token="%SONAR_TOKEN%"
                    '''
                }
            }
        }

        stage('Kalite Kapısı Kontrolü') {
            steps {
                dir('backend') {
                    // dotnet-sonarscanner "end" raporu SonarQube'e yükler ama sunucudaki
                    // işlenmesi (Compute Engine) asenkron — bu yüzden report-task.txt'teki
                    // görev linkini polluyoruz. Kalite Kapısı "OK" değilse pipeline'ı burada
                    // durduruyoruz, tıpkı Selenium testi gibi: Docker deploy adımına hiç gelinmez.
                    powershell '''
                        $reportTask = Get-Content ".sonarqube/out/.sonar/report-task.txt"
                        $ceTaskUrl = ($reportTask | Where-Object { $_ -like "ceTaskUrl=*" }) -replace '^ceTaskUrl=',''
                        $token = (Get-Content "C:/Users/canse/Jenkins/secrets/sonar-token.txt" -Raw).Trim()
                        $authHeader = @{ Authorization = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$token`:")) }

                        $status = "PENDING"
                        $elapsed = 0
                        $timeout = 120
                        $analysisId = $null
                        while ($elapsed -lt $timeout) {
                            $task = Invoke-RestMethod -Uri $ceTaskUrl -Headers $authHeader
                            $status = $task.task.status
                            if ($status -eq "SUCCESS") { $analysisId = $task.task.analysisId; break }
                            if ($status -eq "FAILED" -or $status -eq "CANCELED") {
                                Write-Error "SonarQube analiz gorevi basarisiz: $status"
                                exit 1
                            }
                            Start-Sleep -Seconds 3
                            $elapsed += 3
                        }
                        if (-not $analysisId) {
                            Write-Error "SonarQube analiz gorevi $timeout saniyede tamamlanmadi."
                            exit 1
                        }

                        $gate = Invoke-RestMethod -Uri "http://localhost:9000/api/qualitygates/project_status?analysisId=$analysisId" -Headers $authHeader
                        Write-Host "SonarQube Kalite Kapisi durumu: $($gate.projectStatus.status)"
                        if ($gate.projectStatus.status -ne "OK") {
                            Write-Error "SonarQube Kalite Kapisi basarisiz (durum: $($gate.projectStatus.status)). Detaylar: http://localhost:9000/dashboard?id=agiz-dis-saglik-takip"
                            exit 1
                        }
                    '''
                }
            }
        }

        stage('Birim Testleri') {
            steps {
                dir('test/AgizDisSaglikTakip.UnitTests') {
                    bat 'dotnet test'
                }
            }
        }

        stage('Frontend Bağımlılıklarını Kur') {
            steps {
                dir('frontend') {
                    // Jenkins'in çalışma klasörü .git'ten temiz bir checkout — node_modules
                    // hiç yok (zaten .gitignore'da, olması da gerekmiyor). "npm ci",
                    // package-lock.json'a birebir uyan, CI için önerilen kurulum komutu.
                    bat 'npm ci'
                }
            }
        }

        stage('Frontend Test Sunucusunu Başlat') {
            steps {
                dir('frontend') {
                    // "ng serve" arka planda başlatılıyor — dev sunucusu doğrudan
                    // localhost:5299'a konuşuyor (bkz. environment.development.ts).
                    bat 'start /B npx ng serve --port 4200 > ng-serve-ci.log 2>&1'
                    // Sabit bir süre beklemek yerine (bazen yetmiyor, bazen gereksiz uzun
                    // sürüyor) port cevap verene kadar birkaç saniyede bir kontrol ediyoruz.
                    powershell '''
                        $timeout = 150
                        $elapsed = 0
                        $up = $false
                        while ($elapsed -lt $timeout) {
                            try {
                                $response = Invoke-WebRequest -Uri "http://localhost:4200/" -UseBasicParsing -TimeoutSec 3
                                if ($response.StatusCode -eq 200) { $up = $true; break }
                            } catch {}
                            Start-Sleep -Seconds 3
                            $elapsed += 3
                        }
                        if (-not $up) {
                            Write-Error "Frontend test sunucusu $timeout saniyede ayaga kalkmadi."
                            exit 1
                        }
                        Write-Host "Frontend test sunucusu $elapsed saniyede ayaga kalkti."
                    '''
                }
            }
        }

        stage('Selenium E2E Testi') {
            steps {
                dir('test/AgizDisSaglikTakip.E2ETests') {
                    bat 'dotnet test'
                }
            }
        }

        stage('Docker Image\'larını Derle ve Ayağa Kaldır') {
            steps {
                // .env, git'e hiç girmiyor (gizli anahtarlar içeriyor) — Jenkins'in temiz
                // checkout'unda bulunmaz, bu yüzden repo dışındaki sabit bir kopyadan alınıyor.
                bat 'copy /Y "C:\\Users\\canse\\Jenkins\\secrets\\.env" .env'
                // BACKEND_PORT/FRONTEND_PORT vermiyoruz — docker-compose.yml'in varsayılanı
                // (5299/4300) devreye giriyor, yani doğrudan CANLI portlara deploy ediyor.
                bat 'docker compose up -d --build'
            }
        }
    }

    post {
        always {
            // Test sunucusunu (ng serve) kapat — canlı yığının parçası değil, sadece
            // bu pipeline için açılmıştı.
            powershell '''
                Get-CimInstance Win32_Process -Filter "name='node.exe'" |
                    Where-Object { $_.CommandLine -match "4200" } |
                    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
            '''
        }
    }
}
