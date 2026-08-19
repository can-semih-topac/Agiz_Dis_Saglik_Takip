using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IGoalStatusRepository : IRepository<GoalStatus>
{
    Task<List<GoalStatus>> GetByGoalIdAsync(int goalId);
    Task<List<GoalStatus>> GetLast7DaysByUserIdAsync(int userId);
    // Seri (streak) hesaplaması tüm geçmişe bakmak zorunda olduğu için tarih filtresiz.
    Task<List<GoalStatus>> GetAllByUserIdAsync(int userId);
}
