namespace AgizDisSaglikTakip.Core.Utilities.Security.Hashing;

// BCrypt tek yönlü (one-way) bir hash — AesEncryptionService'in aksine geri çözülemez.
// Şifre her hash'lendiğinde otomatik üretilen farklı bir "salt" kullanılır (aynı şifre farklı
// kullanıcılarda farklı hash üretir), salt'ın kendisi de üretilen hash string'inin içinde saklanır,
// ayrı bir kolon gerekmez.
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
