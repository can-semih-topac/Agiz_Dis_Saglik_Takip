using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IAdminActionLogRepository : IRepository<AdminActionLog>
{
    Task<List<AdminActionLog>> GetRecentAsync(int count);
}
