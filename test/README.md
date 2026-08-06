# Parola Çözme Test Aracı

"parolalar hash değil, geri çözülebilir simetrik şifreyle (AES) saklanıyor" gereksinimini fiilen doğrulamak için.

Uygulamanın kendisinde (`AuthManager.LoginAsync`) bu çözme işlemi zaten **her girişte otomatik** yapılıyor — bu araç, aynı işlemi elle, tek bir kayıt üzerinde, gösterim amaçlı tekrarlıyor. Kalıcı bir özellik değil, ana `backend`/`frontend` klasörlerinden tamamen bağımsız.

## Nasıl çalışır

`DecryptPassword/Program.cs`, backend'deki **gerçek** `AesEncryptionService` sınıfını (Core projesine referans vererek) kullanıyor — yani kod kopyalanmadı, birebir aynı şifre çözme mantığı çalışıyor.

## Şifreli metni nereden alacaksın

SSMS'te (ya da `sqlcmd` ile) şu sorguyu çalıştır:

```sql
SELECT Email, PasswordEncrypted FROM AgizDisSaglikDb.dbo.Users;
```

`PasswordEncrypted` kolonundaki değeri (örnek: `rGxc/xOQ/HvLRxFxwC5t3Ze4pURaw4lfZwPQlMQIcGk=`) kopyala.

## Nasıl çalıştırılır

```
dotnet run --project test/DecryptPassword
```

Terminal sana metni soracak — kopyaladığın `PasswordEncrypted` değerini yapıştırıp Enter'a bas. Ekrana düz metin (çözülmüş) parola basılacak.


## Önemli not

`Program.cs` içindeki `aesKey` sabiti, backend'deki `appsettings.json` → `Encryption:AesKey` değeriyle **aynı** olmalı. O değeri değiştirirsen (anahtar rotasyonu yaparsan) burayı da elle güncellemen gerekir — bu araç appsettings.json'ı otomatik okumuyor, bilinçli olarak basit tutuldu.
