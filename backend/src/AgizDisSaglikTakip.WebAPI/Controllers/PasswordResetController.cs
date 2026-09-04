using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

// AuthController'dan ayrıldı (SonarQube S6960: "birden fazla sorumluluk") — şifre sıfırlama
// adım adım (kod gönder -> kodu doğrula -> yeni şifreyi kaydet) ayrı bir akış, tek bir
// controller'da giriş/kayıt ile karışmasın diye. Route bilerek "api/Auth" olarak SABİT
// bırakıldı (controller adından türetilmedi) — frontend'in kullandığı URL'ler değişmesin diye.
[ApiController]
[Route("api/Auth")]
public class PasswordResetController : ControllerBase
{
    private readonly IAuthService _authService;

    public PasswordResetController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("forgot-password/request-code")]
    public async Task<IActionResult> RequestResetCode(VerifyEmailDto dto)
    {
        var result = await _authService.RequestPasswordResetCodeAsync(dto.Email);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password/verify-code")]
    public async Task<IActionResult> VerifyResetCode(VerifyResetCodeDto dto)
    {
        var result = await _authService.VerifyPasswordResetCodeAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
