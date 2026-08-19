namespace AgizDisSaglikTakip.Business.DTOs.StatusNote;

public class UpdateStatusNoteDto
{
    public string Description { get; set; } = string.Empty;

    // Yeni bir görsel yüklendiyse ikisi de dolu olur, mevcut görsel onunla değiştirilir.
    public Stream? ImageStream { get; set; }
    public string? ImageExtension { get; set; }

    // Yeni görsel yüklenmeden bu true gelirse mevcut görsel kaldırılır.
    public bool RemoveImage { get; set; }
}
