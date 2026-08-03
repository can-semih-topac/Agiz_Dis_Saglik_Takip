# Ağız ve Diş Sağlığı Takip Uygulaması — Proje Gereksinimleri

> TRtek Yazılım Proje Formu (PRJIC20240501) referans dokümanının sadeleştirilmiş halidir.

## Genel Bilgiler

- **Proje No:** PRJIC20240501
- **Amaç:** Kullanıcıların ağız-diş sağlığı alışkanlıklarını (fırçalama, diş ipi, gargara vb.) takip etmesi; hedef belirleyip durum kaydetmesi; öneri alması.
- **Teslim:** Kod + çalışır DB versiyon yönetim aracı (GitLab) üzerinden.

## Teknoloji Yığını (bu projede seçilen)

| Katman | Seçim |
|---|---|
| Backend | .NET 8 ASP.NET Core **Web API** |
| Frontend | **Angular** (ayrı client) |
| Veritabanı | **MSSQL** |
| ORM | Entity Framework Core (**Code-First**) |
| Mimari | N-Katmanlı |
| Prensipler | SOLID, Clean Code |
| Kimlik | **Sıfırdan** (IdentityServer vb. YASAK) |

## Standartlar (formdan)

- Arayüz için hazır HTML tema kullanılabilir.
- SOLID ve Clean Code prensiplerine uyulmalı.
- Tablolar arası FK ilişki yönetimi sağlanmalı.
- DB normalizasyon kurallarına uygun olmalı.
- Kullanıcı yönetimi hazır kütüphaneyle DEĞİL, sıfırdan yazılmalı.

## Başlangıç Kriteri

- [ ] Proje başında **veritabanı şeması** oluşturulup **ekran görüntüsü** olarak teslim edilecek.

## Tamamlanma Kriteri

- [ ] Uygulama kodları + çalışır DB, GitLab üzerinden teslim edilecek.

---

## Sayfalar ve Gereksinimler

### 1. Kullanıcı Kayıt Sayfası
- Mail, parola, ad-soyad, doğum tarihi ile kayıt.
- Parola: min 8 karakter, büyük-küçük harf + rakam içermeli.
- Doğum tarihi takvimden seçilmeli (hatalı tarih engellenmeli).
- Parola tekrar alanı olmalı.
- Mail formatı kontrol edilmeli.
- Eksik alan uyarısı verilmeli.
- Aynı mail ile ikinci kayda izin verilmemeli.
- Başarılı kayıtta HTML formatında bilgilendirme maili gönderilmeli.
- Parola, belirlenen bir anahtar metne göre **şifrelenmiş** saklanmalı.
- Şifreli parola, anahtar metinle **geri çözülebilmeli** (→ hash değil, simetrik şifreleme).

### 2. Giriş Sayfası
- Mail + parola ile giriş.
- Parola karakterleri `*` gösterilmeli.
- Boş alan arayüzde kontrol + uyarı.
- Kayıt ve parola hatırlatma linkleri olmalı.
- Kullanıcı yok / parola yanlış için **farklı** uyarı mesajları.
- Doğru bilgide ana sayfaya yönlendir.

### 3. Parola Hatırlatma Sayfası
- Açılışta sadece mail alanı + doğrulama düğmesi.
- Mail boş kontrolü.
- Doğrulamada mailin DB'de kayıtlı olup olmadığı kontrol edilmeli.
- Kayıtlıysa: parola + parola tekrar alanları gösterilmeli.
- Kayıtlı değilse: "kullanıcı bulunamadı" mesajı.
- Yeni parola, kayıt kriterlerine uygun olmalı.

### 4. Ana Sayfa
- Kullanıcı adı belirgin gösterilmeli.
- Güvenli çıkış düğmesi.
- Profil sayfası linki.
- Ağız-Diş Sağlığı sayfası linki.
- Son 7 günün verileri özet listelenmeli.
- Rastgele seçilen bir öneri gösterilmeli.

### 5. Profil Sayfası
- Mail, parola, ad-soyad, doğum tarihi güncellenebilmeli.
- Yeni parola kayıt kriterlerine uygun olmalı.
- Farklı mail girilirse başka kullanıcıya ait mi kontrol edilmeli.
- Başkasına aitse mail güncellemesine izin verilmemeli.
- Mail formatı kontrolü.
- Eksik alan uyarısı.
- Parola şifreli saklanmalı / geri çözülebilmeli.

### 6. Ağız ve Diş Sağlığı Sayfası
İki sekme: **Durum** (varsayılan seçili) ve **Hedef**.

**Durum sekmesi:**
- Son 7 günün hedef verileri özet listelenmeli.
- Her kayıtlı hedef için: tarih, saat, süre + "uygulandı" bilgisi girilebilen form.
- Açıklama metni + görsel (.jpeg/.png vb.) ile not girilebilen form.
- Rastgele öneri gösterilen alan.

**Hedef sekmesi:**
- Daha önce kaydedilen hedefler listelenmeli.
- Yeni hedef: başlık, açıklama, periyot (zaman + sıklık), önem derecesi (düşük/orta/yüksek).
- Hedef silinebilmeli.
- Silinen hedefin durum kaydı varsa silmeden önce kullanıcı onayı istenmeli.

---

## Önemli Notlar / Dikkat Noktaları

- **Parola simetrik şifreleme:** Form açıkça "geri çözülebilir" istiyor. Bu güvenlik açısından ideal değildir (gerçek dünyada parolalar hash'lenir), ancak form gereği simetrik şifreleme (örn. AES) kullanılacak. Anahtar metin config'de tutulacak.
- **Görsel saklama:** Görseller dosya sistemine kaydedilecek, DB'de yalnızca dosya yolu tutulacak.
- **Kimlik doğrulama:** JWT tabanlı, sıfırdan. Hazır kütüphane yok.
