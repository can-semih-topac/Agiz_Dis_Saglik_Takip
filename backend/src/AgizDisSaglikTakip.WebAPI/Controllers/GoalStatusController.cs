using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalStatusController : ControllerBase
{
    private readonly IGoalStatusService _goalStatusService;

    public GoalStatusController(IGoalStatusService goalStatusService)
    {
        _goalStatusService = goalStatusService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoalStatus(CreateGoalStatusDto dto)
    {
        var result = await _goalStatusService.CreateGoalStatusAsync(this.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("last7days")]
    public async Task<IActionResult> GetLast7Days()
    {
        var result = await _goalStatusService.GetLast7DaysAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
