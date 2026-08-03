using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IGoalRepository : IRepository<Goal>
{
    Task<List<Goal>> GetByUserIdAsync(int userId);
}
