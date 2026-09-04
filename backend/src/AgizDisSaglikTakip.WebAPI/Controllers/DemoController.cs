using AgizDisSaglikTakip.Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

// AuthController'dan ayrıldı (SonarQube S6960) — demo girişi ayrı bir servise (IDemoService)
// dayanıyor ve gerçek kimlik doğrulamayla ilgisi yok. Route bilerek "api/Auth" olarak SABİT
// bırakıldı (controller adından türetilmedi) — frontend'in kullandığı URL değişmesin diye.
[ApiController]
[Route("api/Auth")]
public class DemoController : ControllerBase
{
    private readonly IDemoService _demoService;

    public DemoController(IDemoService demoService)
    {
        _demoService = demoService;
    }

    [HttpPost("demo")]
    public async Task<IActionResult> EnterDemo()
    {
        var result = await _demoService.EnterDemoAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
