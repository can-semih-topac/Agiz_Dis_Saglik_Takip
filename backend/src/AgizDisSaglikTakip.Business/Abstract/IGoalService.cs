using AgizDisSaglikTakip.Business.DTOs.Goal;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IGoalService
{
    Task<ServiceResult<List<GoalDto>>> GetGoalsAsync(int userId);
    Task<ServiceResult> CreateGoalAsync(int userId, CreateGoalDto dto);

    // confirmed=false iken hedefin durum kaydı varsa silme yapılmaz, onay istenir (Data=true döner).
    Task<ServiceResult<bool>> DeleteGoalAsync(int userId, int goalId, bool confirmed);
}
