using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Contact;
using AgizDisSaglikTakip.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgizDisSaglikTakip.WebAPI.Controllers;

// [Authorize] YOK — oturum açmayan ziyaretçiler (Giriş/Kayıt Ol/Şifremi Unuttum ekranları) de kullanabilmeli.
[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] SendContactMessageRequest request)
    {
        byte[]? imageBytes = null;
        string? imageExtension = null;

        if (request.Image != null)
        {
            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream);
            imageBytes = memoryStream.ToArray();
            imageExtension = Path.GetExtension(request.Image.FileName);
        }

        var dto = new SendContactMessageDto
        {
            FullName = request.FullName ?? string.Empty,
            Email = request.Email ?? string.Empty,
            Message = request.Message ?? string.Empty,
            ImageBytes = imageBytes,
            ImageExtension = imageExtension
        };

        var result = await _contactService.SendMessageAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Sadece admin panelinden kullanılacak — controller'da genel [Authorize] olmadığı için
    // burada action bazında ekliyoruz, POST tarafı herkese açık kalmaya devam ediyor.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllMessages()
    {
        var result = await _contactService.GetAllMessagesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
