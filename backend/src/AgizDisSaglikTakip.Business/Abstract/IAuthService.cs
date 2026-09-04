using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IAuthService
{
    Task<ServiceResult> RegisterAsync(RegisterDto dto);
    Task<ServiceResult<LoginResultDto>> LoginAsync(LoginDto dto);
    Task<ServiceResult<LoginResultDto>> GoogleLoginAsync(GoogleLoginDto dto);

    // Şifre hatırlatma 3 adımlı: kod gönder -> kodu doğrula -> yeni şifreyi kaydet.
    Task<ServiceResult> RequestPasswordResetCodeAsync(string email);
    Task<ServiceResult> VerifyPasswordResetCodeAsync(VerifyResetCodeDto dto);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto);

    // Access token süresi dolunca frontend bunu çağırıp kullanıcıyı tekrar giriş yaptırmadan
    // yeni bir access token alır (rotasyonlu: eski refresh token da yenisiyle değiştirilir).
    Task<ServiceResult<LoginResultDto>> RefreshTokenAsync(RefreshTokenDto dto);

    // Verilen refresh token'ı sunucu tarafında iptal eder — sadece localStorage'ı temizlemek
    // (mevcut frontend davranışı) token'ı süresi dolana kadar geçerli bırakırdı.
    Task<ServiceResult> LogoutAsync(RefreshTokenDto dto);
}
