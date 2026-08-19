using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IDemoService _demoService;

    public AuthController(IAuthService authService, IDemoService demoService)
    {
        _authService = authService;
        _demoService = demoService;
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

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
    {
        var result = await _authService.GoogleLoginAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
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

    [HttpPost("demo")]
    public async Task<IActionResult> EnterDemo()
    {
        var result = await _demoService.EnterDemoAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
