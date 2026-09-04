namespace AgizDisSaglikTakip.WebAPI.Models;

public class UpdateStatusNoteRequest
{
    public string? Description { get; set; }
    public IFormFile? Image { get; set; }
    // Nullable: client bu alanı hiç göndermezse (formdata'da eksikse) model binder'ın sessizce
    // false varsayması yerine bunun ayırt edilebilir olması için (S6964).
    public bool? RemoveImage { get; set; }
}
