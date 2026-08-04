using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IGoalStatusService
{
    Task<ServiceResult> CreateGoalStatusAsync(int userId, CreateGoalStatusDto dto);
    Task<ServiceResult<List<GoalStatusDto>>> GetLast7DaysAsync(int userId);
}
