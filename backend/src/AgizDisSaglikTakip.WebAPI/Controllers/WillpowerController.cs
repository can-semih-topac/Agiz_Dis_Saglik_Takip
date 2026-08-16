using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WillpowerController : ControllerBase
{
    private readonly IWillpowerService _willpowerService;

    public WillpowerController(IWillpowerService willpowerService)
    {
        _willpowerService = willpowerService;
    }

    [HttpGet("score")]
    public async Task<IActionResult> GetScore()
    {
        var result = await _willpowerService.GetScoreAsync(this.GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
