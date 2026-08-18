using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IStatusNoteRepository : IRepository<StatusNote>
{
    Task<List<StatusNote>> GetLast7DaysByUserIdAsync(int userId);
    Task<List<StatusNote>> GetAllByUserIdAsync(int userId);
    // Yumuşak silinenler dahil tüm kayıtlar — sadece demo hesabı sıfırlanırken gerçek (kalıcı)
    // temizlik yapabilmek için; normal akışlarda kullanılmaz.
    Task<List<StatusNote>> GetAllByUserIdIncludingDeletedAsync(int userId);
    Task<List<StatusNote>> GetByGoalStatusIdsAsync(IEnumerable<int> goalStatusIds);
}
