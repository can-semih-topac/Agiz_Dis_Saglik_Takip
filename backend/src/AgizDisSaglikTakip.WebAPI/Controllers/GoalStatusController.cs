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

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _goalStatusService.GetAllAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("longest-streaks")]
    public async Task<IActionResult> GetLongestStreaks()
    {
        var result = await _goalStatusService.GetLongestStreaksAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGoalStatus(int id, UpdateGoalStatusDto dto)
    {
        var result = await _goalStatusService.UpdateGoalStatusAsync(this.GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGoalStatus(int id)
    {
        var result = await _goalStatusService.DeleteGoalStatusAsync(this.GetUserId(), id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
