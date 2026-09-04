# Kayıt Defteri

Bu dosya, Claude'un bu proje üzerinde yaptığı her işlemden sonra tarih-saat damgasıyla
kısaca ne yapıldığını kaydettiği bir günlüktür. Amaç: sohbet geçmişi kaybolsa/özetlense
bile yapılan işlerin kronolojik bir izinin projenin kendisinde kalıcı olarak durması.

Format: `## YYYY-AA-GG SS:DD` başlığı + altında 1-3 satırlık kısa açıklama.

---

## 2026-09-04 10:07
Bu kayıt defteri oluşturuldu. Kural: bundan sonra her işlem sonrasında buraya kısa,
tarih-saat damgalı bir not eklenecek — kullanıcı bunu hatırlatmak zorunda kalmayacak.

## 2026-09-04 09:35 (geriye dönük not)
Şifre hashleme geçişinin (AES → BCrypt) gerçek veride doğruluğu doğrulandı:
backend container'ın yeniden derlenip başladığı teyit edildi, 11 kullanıcının
PasswordHash'i kontrol edildi (10'u "$2" ile başlayan geçerli BCrypt hash, 1'i
şifresiz demo şablon hesabı — beklenen), ardından tek kullanımlık bir test
kullanıcısıyla uçtan uca round-trip giriş testi yapıldı (doğru şifre → 200/token,
yanlış şifre → 400/red), test kullanıcısı silinerek temizlendi.

## 2026-09-04 10:27
Login ve şifre sıfırlamaya brute-force koruması eklendi (`AuthManager.cs`): şifre 5 kez
yanlış girilince hesap 15 dakika kilitleniyor (kilitliyken doğru şifre bile reddediliyor),
her denemede kalan hak mesajla bildiriliyor; şifre sıfırlama kodunda da 5 yanlış denemeden
sonra kod otomatik geçersiz kılınıyor (yeni kod istemek gerekiyor). Şifresini sıfırlayan
kullanıcının giriş kilidi de otomatik kalkıyor. Tüm senaryolar (kilitlenme, kilitliyken
doğru şifre reddi, kod deneme sınırı ve otomatik geçersizleşme, reset sonrası kilit
kalkması) container'daki gerçek Redis+DB'ye karşı tek kullanımlık test kullanıcısıyla
uçtan uca doğrulandı, test kullanıcısı temizlendi.

## 2026-09-04 11:27
JWT refresh token mekanizması eklendi. Access token ömrü 60 dk'dan 15 dk'ya düşürüldü,
yeni bir `RefreshTokens` tablosu (yeni migration, sadece hash saklanıyor) ile 7 günlük
refresh token verilmeye başlandı. Rotasyonlu: her yenilemede eski token iptal edilip
yenisi verilir; iptal edilmiş bir token tekrar kullanılmaya çalışılırsa (çalıntı şüphesi)
kullanıcının TÜM oturumları otomatik kapatılıyor. Yeni `/api/Auth/refresh` ve
`/api/Auth/logout` (sunucu tarafında token iptali) endpoint'leri eklendi; demo hesabı da
aynı mekanizmayı kullanıyor. Frontend'de `auth.interceptor.ts` artık 401 alınca arka planda
sessizce refresh deneyip isteği otomatik tekrarlıyor (paralel isteklerde tek refresh
paylaşılıyor), başarısızsa oturumu kapatıp girişe yönlendiriyor. Backend tarafı container'daki
gerçek DB'ye karşı uçtan uca doğrulandı (login → refresh → rotasyon → eski token'ın
reddi/tüm-oturum-iptali → logout → iptal edilen tokenın reddi), test kullanıcısı temizlendi.
Frontend tarafı `ng build` ile tip/derleme hatasız doğrulandı; tarayıcı üzerinde interaktif
401→refresh akışı test EDİLEMEDİ (bu ortamda tarayıcı aracı yok) — kullanıcıya ayrıca belirtildi.

## 2026-09-04 11:58
SonarQube taraması Jenkins pipeline'ına entegre edildi. "Backend Derle" adımı
"Backend Derle ve SonarQube Analizi" olarak genişletildi (dotnet-sonarscanner begin/end
derlemeyi sarmalıyor, zaten kurulu olan dotnet-sonarscanner global tool'u kullanıyor),
ardından yeni bir "Kalite Kapısı Kontrolü" adımı ekledim — SonarQube plugin'i Jenkins'e
kurmadan (sistem ayarlarına dokunmamak icin bilinçli tercih), SonarQube'ün Web API'sini
PowerShell ile polluyor, Kalite Kapısı "OK" değilse pipeline'ı Selenium testiyle aynı
mantıkla durdurup Docker deploy adımına hiç geçmiyor. CI için ayrı bir SonarQube token'ı
üretip (admin/admin ile, ki bunun hâlâ varsayılan olduğunu kullanıcıya ayrıca flagledim)
repo dışına (C:\Users\canse\Jenkins\secrets\sonar-token.txt) kaydettim. Tüm zinciri
(begin → build → end → Kalite Kapısı polling) hem Bash hem PowerShell ile yerel olarak
uçtan uca test ettim, ikisi de "ANALYSIS SUCCESSFUL" ve "Kalite Kapisi durumu: OK" ile
sonuçlandı. Jenkinsfile'ı gerçek Jenkins linter'ıyla doğrulayamadım (kimlik doğrulama
gerektiriyor) — sözdizimini elle inceleyip aynı script'i PowerShell'de birebir çalıştırarak
doğruladım, gerçek doğrulama Jenkins'in bir sonraki push'ta otomatik tetiklenen build'i ile yapılacak.
