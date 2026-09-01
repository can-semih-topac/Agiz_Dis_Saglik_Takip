// Jenkins'in çalıştıracağı pipeline — "Pipeline as Code": bu betik Jenkins arayüzüne
// gömülü değil, repo'nun bir parçası, yani Source Control'den takip edilebiliyor.
//
// "Selenium E2E Testi" adımı, localhost:5299'daki backend'in zaten ayakta olmasını
// bekliyor (standart 3'lü canlı yığının parçası) — Jenkins onu başlatmıyor.
//
// Son adım Docker image'larını derleyip ayağa kaldırıyor — ama 5299/4300 (canlı yığının
// portları) DEĞİL, BACKEND_PORT/FRONTEND_PORT ortam değişkenleriyle 5301/4301'e alınıyor
// (docker-compose.yml'deki "${BACKEND_PORT:-5299}" gibi tanımlar sayesinde). Yani bu adım
// canlıya hiç dokunmuyor, sadece "image doğru derleniyor mu, container doğru ayağa kalkıyor
// mu"yu her push'ta otomatik doğruluyor. Container'ların canlıdaki gerçek 5299/4300'ü
// devralması (dotnet watch/npx serve'ün emekliye ayrılması) ayrı, bilinçli bir adım olarak kalacak.
pipeline {
    agent any

    stages {
        stage('Backend Derle') {
            steps {
                dir('backend') {
                    // "-o ci-build": canlıda sürekli çalışan "dotnet watch" kendi bin/Debug
                    // çıktısını sürekli kilitli tutuyor — Jenkins AYNI klasöre yazmaya
                    // çalışırsa her seferinde çakışır. Ayrı bir klasöre derleyerek bunu
                    // tamamen önlüyoruz, canlı sürece hiç dokunmuyoruz.
                    bat 'dotnet build -o ci-build'
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
                // BACKEND_PORT/FRONTEND_PORT burada set edilip AYNI bat bloğunda kullanılıyor —
                // Jenkins'te her "bat" adımı ayrı bir cmd.exe olduğu için "set" bir sonrakine miras kalmaz.
                // Docker Desktop, Jenkins başlatıldıktan SONRA kurulduğu için "docker" PATH'te yok,
                // tam yol kullanmak Jenkins'i yeniden başlatma ihtiyacını ortadan kaldırıyor.
                bat '''
                    set BACKEND_PORT=5301
                    set FRONTEND_PORT=4301
                    "C:\\Users\\canse\\AppData\\Local\\Programs\\DockerDesktop\\resources\\bin\\docker.exe" compose up -d --build
                '''
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
