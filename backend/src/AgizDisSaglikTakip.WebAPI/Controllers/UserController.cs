using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.User;
using AgizDisSaglikTakip.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService.GetProfileAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(this.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var result = await _userService.ChangePasswordAsync(this.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await _userService.DeleteAccountAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Sadece admin panelinden kullanılacak — controller'daki genel [Authorize] yetmiyor, admin şartı ekliyoruz.
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser(CreateUserByAdminDto dto)
    {
        var result = await _userService.CreateUserByAdminAsync(this.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserByAdminAsync(this.GetUserId(), id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
