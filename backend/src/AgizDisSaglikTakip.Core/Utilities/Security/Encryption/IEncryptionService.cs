namespace AgizDisSaglikTakip.Core.Utilities.Security.Encryption;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
