using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<LoginResultDto>> LoginAsync(LoginDto dto);
    Task<ServiceResult> VerifyEmailForPasswordResetAsync(string email);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto);
}
