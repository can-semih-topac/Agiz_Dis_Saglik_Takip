using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Goal;
using AgizDisSaglikTakip.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalController : ControllerBase
{
    private readonly IGoalService _goalService;

    public GoalController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGoals() //Hedefleri getir
    {
        var result = await _goalService.GetGoalsAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGoal(CreateGoalDto dto) // yeni hedef yap
    {
        var result = await _goalService.CreateGoalAsync(this.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGoal(int id, UpdateGoalDto dto) // hedefi düzenle
    {
        var result = await _goalService.UpdateGoalAsync(this.GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGoal(int id, [FromQuery] bool confirmed = false) // hedef sil
    {
        var result = await _goalService.DeleteGoalAsync(this.GetUserId(), id, confirmed);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/pause")]
    public async Task<IActionResult> PauseGoal(int id, StartGoalPauseDto dto)
    {
        var result = await _goalService.PauseGoalAsync(this.GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/resume")]
    public async Task<IActionResult> ResumeGoal(int id)
    {
        var result = await _goalService.ResumeGoalAsync(this.GetUserId(), id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
