using AgizDisSaglikTakip.Business.DTOs.AdminActionLog;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IAdminActionLogService
{
    Task<ServiceResult<List<AdminActionLogDto>>> GetRecentAsync();
    Task RecordAsync(string adminEmail, string action, string targetEmail);
}
