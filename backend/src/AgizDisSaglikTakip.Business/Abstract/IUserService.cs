using AgizDisSaglikTakip.Business.DTOs.User;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IUserService
{
    Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId);
    Task<ServiceResult> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<ServiceResult> DeleteAccountAsync(int userId);
}
