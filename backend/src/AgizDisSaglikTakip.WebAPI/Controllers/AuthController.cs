using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password/verify")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
    {
        var result = await _authService.VerifyEmailForPasswordResetAsync(dto.Email);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
