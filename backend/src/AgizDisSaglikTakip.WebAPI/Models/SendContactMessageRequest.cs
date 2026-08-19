namespace AgizDisSaglikTakip.WebAPI.Models;

// Business'taki SendContactMessageDto'dan farklı: burada IFormFile var, çünkü bu tip
// sadece web/HTTP katmanına (multipart/form-data) ait, Business'ın bunu hiç görmemesi gerekiyor.
public class SendContactMessageRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public IFormFile? Image { get; set; }
}
