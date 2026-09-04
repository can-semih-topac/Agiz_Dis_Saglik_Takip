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

## 2026-09-04 16:06
İlk SonarQube taramasının bulduğu 19 bulgunun (1 güvenlik açığı + 18 code smell) çoğu
düzeltildi. Güvenlik açığı: Dockerfile artık root yerine .NET 8 imajının gömülü "app"
kullanıcısıyla (non-root) çalışıyor — önce mevcut upload volume'ünün sahipliğini bu
kullanıcıya devrettim, sonra gerçek bir dosya yükleme testiyle yazma iznini doğruladım.
Mekanik düzeltmeler: Program.cs'te sync->async (MigrateAsync/RunAsync) ve hardcoded
Elasticsearch URI'si appsettings.json'a taşındı, UserManager'da iç içe if birleştirildi,
LogManager'da dizi static readonly'e alındı, DbLoggerProvider dispose deseni (sealed +
GC.SuppressFinalize) düzeltildi, UpdateStatusNoteRequest/Dto RemoveImage nullable yapıldı
(under-posting koruması), iki "yanlış pozitif" S125 bulgusu (aslında kod değil, sonunda
";" olan Türkçe yorumlar) düzeltildi. Daha kapsamlı: WillpowerManager.ComputeScoreAsOf
(Cognitive Complexity 24->15 sınırı) iki yardımcı metoda (ComputeGoalContribution,
ResolveStreakReferenceDate) bölündü — demo hesabıyla skor/geçmiş uçtan uca test edilip
davranışın değişmediği doğrulandı. AuthController (S6960, "çok fazla sorumluluk") üçe
bölündü: AuthController (giriş/kayıt/refresh/logout), PasswordResetController (şifre
sıfırlama), DemoController (demo girişi) — route'lar bilerek "api/Auth" sabit tutuldu,
frontend'de hiçbir değişiklik gerekmedi, tüm endpoint'ler smoke test edildi.
Bilinçli olarak DÜZELTİLMEYEN 4 bulgu SonarQube'de "Won't Fix" olarak işaretlendi
(gerekçesiyle): AuthManager/DemoManager/UserManager'daki 8-9 parametreli constructor'lar
(DI'da tekil-sorumluluklu servislere ayrılmış, facade'e sıkıştırmak regresyon riski
katardı) ve bir migration dosyasındaki stil önerisi (uygulanmış migration'lara dokunulmuyor).
Ayrıca fark ettim: yeni oluşturulan SonarQube projelerinde varsayılan "Sonar way" kalite
kapısı "yeni kodun test coverage'ı >=%80" şartı içeriyor — projede hiç coverage altyapısı
olmadığı için bu her build'de otomatik FAIL ederdi. Coverage şartı olmayan özel bir kapı
("CI-Gate-No-Coverage") oluşturup projeye atadım; coverage entegrasyonu ayrı, gelecekteki
bir iş. Tarama sonrası: 0 bug, 0 vulnerability, sadece 3 won't-fix code smell kaldı,
Kalite Kapısı "OK".

## 2026-09-04 17:01
Backend'e birim (unit) test eklendi. Yeni proje: test/AgizDisSaglikTakip.UnitTests
(xUnit + Moq, E2E test projesiyle aynı konvansiyon). En riskli/karmaşık iş mantığı
hedeflendi: AuthBusinessRules (e-posta/telefon/şifre doğrulama), StreakCalculator (seri
hesaplama, duraklatma köprüleme), WillpowerManager (irade puanı motoru — bugün refactor
edilen ComputeScoreAsOf/ComputeGoalContribution'ın davranışı bozulmadığı kanıtlandı) ve
AuthManager (giriş kilidi, refresh token rotasyonu/çalıntı tespiti, şifre sıfırlama kod
deneme sınırı — bu oturumda eklenen tüm güvenlik mekanizmaları). AuthManager testlerinde
IDistributedCache için mock yerine gerçek (Redis'siz, bellek içi) MemoryDistributedCache,
hash/token üretimi için gerçek BCryptPasswordHasher/JwtTokenService kullanıldı — sahte
repository'ler (FakeUserRepository, FakeRefreshTokenRepository) da elle yazıldı çünkü
rotasyon senaryoları çok adımlı/durum bağımlı, Moq ile taklit etmek kırılgan olurdu.
56 testten ilk çalıştırmada 1'i GERÇEK bir bug buldu: AuthBusinessRules.IsValidEmailFormat
boş e-postayla çağrılınca (FormatException değil) yakalanmamış bir ArgumentException
fırlatıp backend'i çökertiyordu — düzeltildi (erken boş/whitespace kontrolü eklendi).
Jenkinsfile'a "Birim Testleri" adımı eklendi (Kalite Kapısı'ndan sonra, Selenium'dan önce
— hızlı başarısız olsun diye). Backend hem normal build hem test run ile doğrulandı,
56/56 test geçiyor.

## 2026-09-04 17:20
"Kayıtları düzenleme özelliği ekle" maddesi tamamlandı — inceleyince günlük kayıtların
(GoalStatus) ve notların (StatusNote) zaten düzenlenebildiği, düzenlenemeyen tek şeyin
hedeflerin kendisi (Goal) olduğu ortaya çıktı; kullanıcıya sorup "hedef düzenleme"
olduğunu netleştirdim. Backend: UpdateGoalDto + IGoalService.UpdateGoalAsync +
GoalController PUT /api/goal/{id} eklendi, sahiplik kontrolü (userId eşleşmesi) ve
CreateGoalAsync ile aynı doğrulama kuralları (ortak ValidateGoalFields yardımcı
metoduna çıkarıldı, tekrar önlendi). Frontend: health sayfasındaki "Yeni Hedef" formu
"düzenleme moduna" da girebiliyor artık — hedef listesindeki "Düzenle" butonuna
basınca form mevcut değerlerle doluyor, kaydedince create yerine update çağrılıyor,
"Vazgeç" ile iptal edilebiliyor. Backend'i gerçek HTTP istekleriyle uçtan uca test
ettim: güncelleme başarılı, başka kullanıcının hedefini düzenleme denemesi "Hedef
bulunamadı" ile reddedildi (sahiplik kontrolü çalışıyor), boş başlıkla validasyon
doğru çalıştı. Frontend `ng build` ile tip hatasız derlendi; tarayıcı aracı bu ortamda
olmadığı için UI'ı görsel olarak deneyemedim.

## 2026-09-04 17:24
Push sonrası Jenkins build #32 Kalite Kapısı'nda FAIL etti (Birim Testleri adımına hiç
gelinmedi) — sebep: yeni eklediğim UpdateGoalAsync, GoalManager.cs içinde "Hedef
bulunamadı." literalini 4. kez tekrarladı, SonarQube'ün S1192 kuralı ("aynı string
3+ kez varsa sabit yap") tetiklendi. ErrorMessages.cs'e GoalNotFound sabiti eklendi,
GoalManager.cs (4 yer) ve tutarlılık için GoalStatusManager.cs'deki (2 yer) aynı
literal de bu sabite taşındı. Yerel taramayla doğrulandı: uyarı gitti, Kalite Kapısı
tekrar "OK". Bu, Jenkinsfile'a eklediğim Kalite Kapısı'nın gerçekten işe yaradığının
kanıtı — deploy'a gelmeden hatayı yakaladı.
