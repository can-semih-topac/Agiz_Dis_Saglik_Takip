namespace AgizDisSaglikTakip.Business.DTOs.StatusNote;

public class StatusNoteDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}
