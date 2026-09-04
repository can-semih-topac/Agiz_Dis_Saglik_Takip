namespace AgizDisSaglikTakip.Entities;

// Access token (JWT) kısa ömürlü (15 dk) — kullanıcı sürekli yeniden giriş yapmak zorunda
// kalmasın diye süresi dolunca bu token ile sessizce yenileniyor (bkz. AuthManager.RefreshTokenAsync).
// Kendisi JWT değil, yüksek entropili rastgele bir metin; DB'de SADECE hash'i tutuluyor —
// veritabanı bir şekilde ele geçirilse bile buradan kullanılabilir bir oturum çıkarılamasın diye.
public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    // Rotasyon: her başarılı yenilemede bu token iptal edilip yerine yenisi verilir (bkz.
    // RefreshTokenAsync). Dolu bir RevokedAt, iptal edilmiş bir token'ın TEKRAR kullanılmaya
    // çalışıldığını (olası çalıntı) tespit edebilmemizi sağlar.
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}
