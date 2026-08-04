using AgizDisSaglikTakip.Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuggestionController : ControllerBase
{
    private readonly ISuggestionService _suggestionService;

    public SuggestionController(ISuggestionService suggestionService)
    {
        _suggestionService = suggestionService;
    }

    [HttpGet("random")]
    public async Task<IActionResult> GetRandom()
    {
        var result = await _suggestionService.GetRandomAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
