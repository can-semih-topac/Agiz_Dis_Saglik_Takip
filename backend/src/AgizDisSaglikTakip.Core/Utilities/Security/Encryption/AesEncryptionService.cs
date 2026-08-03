using System.Security.Cryptography;
using System.Text;

namespace AgizDisSaglikTakip.Core.Utilities.Security.Encryption;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // IV, şifreli metnin başına ekleniyor; çözerken buradan geri okunacak.
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var fullBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        var cipherBytes = new byte[fullBytes.Length - ivLength];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, ivLength);
        Buffer.BlockCopy(fullBytes, ivLength, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
