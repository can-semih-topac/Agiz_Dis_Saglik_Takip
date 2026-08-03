using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IGoalStatusRepository : IRepository<GoalStatus>
{
    Task<List<GoalStatus>> GetByGoalIdAsync(int goalId);
    Task<List<GoalStatus>> GetLast7DaysByUserIdAsync(int userId);
}
