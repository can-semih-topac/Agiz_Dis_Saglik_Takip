using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.Core.Utilities.Security.Hashing;
using AgizDisSaglikTakip.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.Business.Seed;

// Eski, geri döndürülebilir AES şifrelemesiyle saklanmış şifreleri çözüp tek yönlü BCrypt
// hash'ine çeviriyor. Migration'lar/DemoDataSeeder gibi idempotent: BCrypt hash'leri her zaman
// "$2" ile başladığından, bu ön eke sahip kayıtlar tekrar işlenmiyor — taşınmış bir kullanıcı
// için bu adım hep "yapacak iş yok" bulup anında çıkıyor.
//
// Kalıcı olarak (geçici değil) bırakıldı: native SQL Server'daki (güvenlik ağı) kopya hâlâ eski
// AES formatında — bir gün oradan geri yüklenirse, bu adım bir sonraki açılışta o kullanıcıları
// da otomatik olarak hash'e taşır. Bu yüzden AesEncryptionService/IEncryptionService/AesKey de
// bilinçli olarak kaldırılmadı.
public static class LegacyPasswordMigrator
{
    public static async Task MigrateAsync(AppDbContext context, IEncryptionService encryptionService, IPasswordHasher passwordHasher)
    {
        // IgnoreQueryFilters: AppDbContext'teki "silinmemiş kullanıcılar" global filtresi
        // olmadan TÜM kayıtları tarıyoruz — yumuşak silinmiş bir kullanıcının şifresi de eski
        // AES formatında kalıp geride unutulmasın diye (ileride geri yüklenirse bozuk kalırdı).
        var usersToMigrate = await context.Users
            .IgnoreQueryFilters()
            .Where(u => u.PasswordHash != null && !u.PasswordHash.StartsWith("$2"))
            .ToListAsync();

        foreach (var user in usersToMigrate)
        {
            var plainPassword = encryptionService.Decrypt(user.PasswordHash!);
            user.PasswordHash = passwordHasher.Hash(plainPassword);
        }

        if (usersToMigrate.Count > 0)
            await context.SaveChangesAsync();
    }
}
