using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IGoalStatusService
{
    // Dönen int, oluşturulan kaydın Id'si — StatusNote'u aynı kayda bağlayabilmek için.
    Task<ServiceResult<int>> CreateGoalStatusAsync(int userId, CreateGoalStatusDto dto);
    Task<ServiceResult<List<GoalStatusDto>>> GetLast7DaysAsync(int userId);
    Task<ServiceResult<List<LongestStreakDto>>> GetLongestStreaksAsync(int userId);
}
