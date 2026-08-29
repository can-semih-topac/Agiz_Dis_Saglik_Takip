using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Log;
using AgizDisSaglikTakip.Core.Utilities.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LogController : ControllerBase
{
    private readonly ILogService _logService;

    public LogController(ILogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var result = await _logService.GetRecentAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(ServiceResult<List<LogDto>>.Fail("Arama kelimesi gerekli."));

        var result = await _logService.SearchAsync(q);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reindex")]
    public async Task<IActionResult> Reindex()
    {
        var result = await _logService.ReindexAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
