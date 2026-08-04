namespace AgizDisSaglikTakip.Business.DTOs.StatusNote;

public class CreateStatusNoteDto
{
    public string Description { get; set; } = string.Empty;

    // İkisi de doluysa görsel kaydedilir, ikisi de null ise sadece metin notu oluşturulur.
    public Stream? ImageStream { get; set; }
    public string? ImageExtension { get; set; }
}
