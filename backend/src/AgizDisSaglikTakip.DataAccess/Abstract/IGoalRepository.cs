using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IGoalRepository : IRepository<Goal>
{
    Task<List<Goal>> GetByUserIdAsync(int userId);
    // Yumuşak silinenler dahil tüm kayıtlar — sadece demo hesabı sıfırlanırken gerçek (kalıcı)
    // temizlik yapabilmek için; normal akışlarda kullanılmaz.
    Task<List<Goal>> GetByUserIdIncludingDeletedAsync(int userId);
}
