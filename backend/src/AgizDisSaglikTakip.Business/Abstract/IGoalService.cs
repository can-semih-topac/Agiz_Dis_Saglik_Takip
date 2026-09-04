using AgizDisSaglikTakip.Business.DTOs.Goal;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IGoalService
{
    Task<ServiceResult<List<GoalDto>>> GetGoalsAsync(int userId);
    Task<ServiceResult> CreateGoalAsync(int userId, CreateGoalDto dto);
    Task<ServiceResult> UpdateGoalAsync(int userId, int goalId, UpdateGoalDto dto);

    // confirmed=false iken hedefin durum kaydı varsa silme yapılmaz, onay istenir (Data=true döner).
    Task<ServiceResult<bool>> DeleteGoalAsync(int userId, int goalId, bool confirmed);

    // Duraklatma süresi boyunca seri bozulmaz/ceza uygulanmaz (bkz. StreakCalculator).
    Task<ServiceResult> PauseGoalAsync(int userId, int goalId, StartGoalPauseDto dto);
    Task<ServiceResult> ResumeGoalAsync(int userId, int goalId);
}
