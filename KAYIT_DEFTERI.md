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
