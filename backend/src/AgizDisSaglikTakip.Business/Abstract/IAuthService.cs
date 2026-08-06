using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<LoginResultDto>> LoginAsync(LoginDto dto);

    // Parola hatırlatma 3 adımlı: kod gönder -> kodu doğrula -> yeni parolayı kaydet.
    Task<ServiceResult> RequestPasswordResetCodeAsync(string email);
    Task<ServiceResult> VerifyPasswordResetCodeAsync(VerifyResetCodeDto dto);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto);
}
