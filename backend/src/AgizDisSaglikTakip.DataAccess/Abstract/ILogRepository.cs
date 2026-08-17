using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface ILogRepository : IRepository<Log>
{
    Task<List<Log>> GetRecentAsync(int count);
}
