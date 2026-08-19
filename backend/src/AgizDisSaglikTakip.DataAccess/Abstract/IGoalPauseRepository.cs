using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IGoalPauseRepository : IRepository<GoalPause>
{
    // Bir hedefin o an açık (EndDate = null) duraklatma kaydı, en fazla bir tane olabilir.
    Task<GoalPause?> GetActivePauseAsync(int goalId);
    // Seri/puan hesaplamasında geçmiş dahil tüm duraklatma aralıklarına ihtiyaç var.
    Task<List<GoalPause>> GetAllByUserIdAsync(int userId);
}
