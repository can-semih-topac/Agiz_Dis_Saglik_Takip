using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IGoalStatusService
{
    // Dönen int, oluşturulan kaydın Id'si — StatusNote'u aynı kayda bağlayabilmek için.
    Task<ServiceResult<int>> CreateGoalStatusAsync(int userId, CreateGoalStatusDto dto);
    Task<ServiceResult<List<GoalStatusDto>>> GetLast7DaysAsync(int userId);
    Task<ServiceResult<List<GoalStatusDto>>> GetAllAsync(int userId);
    Task<ServiceResult<List<LongestStreakDto>>> GetLongestStreaksAsync(int userId);
    // Yanlış girilen tarih/saat/süreyi düzeltmek için — hangi hedefe bağlı olduğu değiştirilemez.
    Task<ServiceResult> UpdateGoalStatusAsync(int userId, int id, UpdateGoalStatusDto dto);
    // Yanlışlıkla "yapıldı" işaretlenen bir kaydı geri almak için — kayıt silinir, hedef tekrar yapılması gerekenlere döner.
    Task<ServiceResult<bool>> DeleteGoalStatusAsync(int userId, int id);
}
