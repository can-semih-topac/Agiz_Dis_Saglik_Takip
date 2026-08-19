using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.StatusNote;
using AgizDisSaglikTakip.WebAPI.Extensions;
using AgizDisSaglikTakip.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatusNoteController : ControllerBase
{
    private readonly IStatusNoteService _statusNoteService;

    public StatusNoteController(IStatusNoteService statusNoteService)
    {
        _statusNoteService = statusNoteService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStatusNote([FromForm] CreateStatusNoteRequest request)
    {
        var dto = new CreateStatusNoteDto
        {
            Description = request.Description ?? string.Empty,
            ImageStream = request.Image?.OpenReadStream(),
            ImageExtension = request.Image != null ? Path.GetExtension(request.Image.FileName) : null,
            GoalStatusId = request.GoalStatusId
        };

        var result = await _statusNoteService.CreateStatusNoteAsync(this.GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatusNote(int id, [FromForm] UpdateStatusNoteRequest request)
    {
        var dto = new UpdateStatusNoteDto
        {
            Description = request.Description ?? string.Empty,
            ImageStream = request.Image?.OpenReadStream(),
            ImageExtension = request.Image != null ? Path.GetExtension(request.Image.FileName) : null,
            RemoveImage = request.RemoveImage
        };

        var result = await _statusNoteService.UpdateStatusNoteAsync(this.GetUserId(), id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("last7days")]
    public async Task<IActionResult> GetLast7Days()
    {
        var result = await _statusNoteService.GetLast7DaysAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _statusNoteService.GetAllAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
