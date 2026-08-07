namespace AgizDisSaglikTakip.Business.DTOs.Contact;

public class SendContactMessageDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public byte[]? ImageBytes { get; set; }
    public string? ImageExtension { get; set; }
}
