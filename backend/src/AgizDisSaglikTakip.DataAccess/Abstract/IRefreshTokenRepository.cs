using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    // İptal edilmiş bir token'ın tekrar kullanılmaya çalışılması olası bir çalıntı sinyalidir —
    // önlem olarak kullanıcının TÜM oturumlarını (tüm cihazlardaki aktif token'larını) kapatmak için.
    Task RevokeAllActiveForUserAsync(int userId);

    // Süresi geçmiş kayıtları tabloda biriktirmemek için — her yeni token verilişinde çağrılır.
    Task DeleteExpiredForUserAsync(int userId);
}
