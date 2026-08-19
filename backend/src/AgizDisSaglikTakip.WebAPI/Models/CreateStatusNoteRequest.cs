namespace AgizDisSaglikTakip.WebAPI.Models;

// Business'taki CreateStatusNoteDto'dan farklı: burada IFormFile var, çünkü bu tip
// sadece web/HTTP katmanına (multipart/form-data) ait, Business'ın bunu hiç görmemesi gerekiyor.
public class CreateStatusNoteRequest
{
    // Nullable bırakıyoruz: [ApiController] boş form alanlarını "eksik" sayıp kendi
    // otomatik doğrulamasını devreye sokuyor; biz bu kontrolü StatusNoteManager'da
    // kendimiz yapıp tutarlı bir ServiceResult mesajı döndürmek istiyoruz.
    public string? Description { get; set; }
    public IFormFile? Image { get; set; }
    public int? GoalStatusId { get; set; }
}
