using AgizDisSaglikTakip.Business.DTOs.Log;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface ILogService
{
    Task<ServiceResult<List<LogDto>>> GetRecentAsync();
}
