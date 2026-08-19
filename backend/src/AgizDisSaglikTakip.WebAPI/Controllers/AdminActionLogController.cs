using AgizDisSaglikTakip.Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminActionLogController : ControllerBase
{
    private readonly IAdminActionLogService _adminActionLogService;

    public AdminActionLogController(IAdminActionLogService adminActionLogService)
    {
        _adminActionLogService = adminActionLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var result = await _adminActionLogService.GetRecentAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
