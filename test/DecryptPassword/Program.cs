using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;

// DİKKAT: Bu, backend'deki appsettings.json -> "Encryption:AesKey" değeriyle
// AYNI olmak zorunda. O değeri değiştirirsen burayı da güncelle.
// (backend/src/AgizDisSaglikTakip.WebAPI/appsettings.json içindeki değer)
const string aesKey = "Wb5qMOvrvoLErFHUqJs7wfd/27OsqhTcV6U3ng4E8Hw=";

Console.WriteLine("=== Parola Çözme Aracı (tek seferlik test) ===");
Console.WriteLine("Users tablosundaki PasswordEncrypted değerini yapıştır ve Enter'a bas:");
Console.WriteLine();

var cipherText = Console.ReadLine();

if (string.IsNullOrWhiteSpace(cipherText))
{
    Console.WriteLine("Boş değer girildi, çıkılıyor.");
    return;
}

try
{
    var encryptionService = new AesEncryptionService(aesKey);
    var plainText = encryptionService.Decrypt(cipherText.Trim());

    Console.WriteLine();
    Console.WriteLine($"Çözülen (düz metin) parola: {plainText}");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine($"Çözülemedi. Muhtemel sebep: yanlış/eksik yapıştırılmış metin ya da AES anahtarı uyuşmuyor.");
    Console.WriteLine($"Hata detayı: {ex.Message}");
}
