# Ağız ve Diş Sağlığı Takip Uygulaması

Kullanıcıların diş fırçalama, diş ipi kullanımı, diş hekimi kontrolü gibi ağız-diş sağlığı alışkanlıklarını takip edip alışkanlık serilerini (streak) ve "irade puanı"nı görebildiği, tam ölçekli bir web uygulaması. TRtek stajı kapsamında sıfırdan geliştirildi — sadece işlevsel bir CRUD uygulaması değil, gerçek bir yazılım ekibinde kullanılan araç ve pratiklerin uçtan uca uygulanmasını hedefleyen bir proje.

**Canlı demo:** [ads.cansemihtopac.com](https://ads.cansemihtopac.com) — kayıt olmadan "Tanıtımı Göster" ile örnek verilerle deneyebilirsiniz.

---

## Kullanılan Teknolojiler

### Backend
- **.NET 8 Web API** — katmanlı mimari (Entities / Core / DataAccess / Business / WebAPI)
- **Entity Framework Core + MSSQL** — uygulama ilk açılışta migration'ları otomatik uygular, elle kurulum gerekmez
- **JWT + Google ile Giriş** — kimlik doğrulama
- **Redis** — kısa ömürlü verilerin (şifre sıfırlama kodları) önbelleklenmesi
- **ElasticSearch** — admin panelindeki log kayıtlarında tam metin arama
- **Sentry** — üretimde yakalanan hataların otomatik izlenmesi

### Frontend
- **Angular 19**
- **PrimeNG** — arayüz bileşen kütüphanesi
- **Transloco** — çoklu dil desteği (TR/EN)
- **PWA** (Angular Service Worker) — mobil cihaza "uygulama gibi" eklenebilir
- **Duyarlı (responsive) tasarım** — mobil uyumlu arayüz
- **Açık / Koyu / Otomatik tema** — CSS değişkenleriyle uçtan uca tema desteği

### DevOps & Kalite
- **Docker & Docker Compose** — backend, frontend, MSSQL, Redis, ElasticSearch container'larda; tek komutla (`docker compose up`) tüm sistem ayağa kalkıyor
- **Jenkins (CI/CD)** — GitHub push'unda webhook ile otomatik tetiklenen pipeline: derleme → E2E test → **testler geçerse canlıyı otomatik günceller**
- **Selenium** — kullanıcı arayüzü üzerinden uçtan uca (E2E) test otomasyonu
- **SonarQube** — statik kod analizi (güvenlik açığı ve kod kokusu taraması)
- **Cloudflare Tunnel** — yerel geliştirme makinesinin güvenli şekilde herkese açık bir adrese (bu repodaki demo linki) yayınlanması

---

## Mimari

```
GitHub push
    │
    ▼
Jenkins (webhook ile tetiklenir)
    │  derle → Selenium E2E testi
    ▼  (testler geçerse)
docker compose up -d --build
    │
    ├── backend (.NET 8 API)
    ├── frontend (Angular, Nginx ile sunulur)
    ├── db (MSSQL)
    ├── redis
    └── elasticsearch
```

Her servis kendi container'ında izole çalışıyor; `restart: always` sayesinde makine ya da Docker yeniden başlasa bile elle müdahale gerekmeden ayağa kalkıyor.

---

## Özellikler

- Alışkanlık (hedef) oluşturma — günlük/haftalık/aylık periyot, önem derecesi, süreli ya da "yapıldı" tipi takip
- Takvim görünümü, en uzun seri (streak) istatistikleri, "irade puanı"
- Durum notlarına fotoğraf ekleme
- Admin paneli: kullanıcı yönetimi, iletişim mesajları, log arama (ElasticSearch destekli)
- "Bize Ulaşın" formu — hem e-posta hem veritabanı kaydı
- Google ile tek tıkla giriş / hesap oluşturma

---

## Yerel Kurulum

```bash
git clone <repo-url>
cd Agiz_Dis_Saglik_Takip
cp .env.example .env   # gerekli değerleri doldurun
docker compose up -d --build
```

Backend `localhost:5299`, frontend `localhost:4300` üzerinden ayağa kalkar. İlk açılışta veritabanı şeması ve örnek demo verisi otomatik oluşturulur.
