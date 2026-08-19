namespace AgizDisSaglikTakip.Core.Utilities.Email;

public class EmailMessage
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? ReplyToEmail { get; set; }
    public string? AttachmentFileName { get; set; }
    public byte[]? AttachmentBytes { get; set; }
}
