using AgizDisSaglikTakip.Business.DTOs.Willpower;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IWillpowerService
{
    Task<ServiceResult<WillpowerScoreDto>> GetScoreAsync(int userId);
}
