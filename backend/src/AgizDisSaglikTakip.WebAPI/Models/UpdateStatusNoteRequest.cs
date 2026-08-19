namespace AgizDisSaglikTakip.WebAPI.Models;

public class UpdateStatusNoteRequest
{
    public string? Description { get; set; }
    public IFormFile? Image { get; set; }
    public bool RemoveImage { get; set; }
}
