// Jenkins'in çalıştıracağı pipeline — "Pipeline as Code": bu betik Jenkins arayüzüne
// gömülü değil, repo'nun bir parçası, yani Source Control'den takip edilebiliyor.
//
// Şimdilik sadece derleme + test (deploy adımı YOK — canlıdaki backend/frontend
// süreçlerine bilerek dokunmuyoruz, bu ayrı ve daha dikkatli ele alınması gereken bir adım).
//
// "Selenium E2E Testi" adımı, localhost:5299'daki backend'in zaten ayakta olmasını
// bekliyor (standart 3'lü canlı yığının parçası) — Jenkins onu başlatmıyor.
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
