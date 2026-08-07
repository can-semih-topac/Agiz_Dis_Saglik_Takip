namespace AgizDisSaglikTakip.Core.Utilities.Email;

public interface IEmailService
{
    Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody);

    // Ek olarak yanıt-adresi (reply-to) ve dosya eki gönderebilmek için — ör. iletişim formu.
    Task SendAsync(EmailMessage email);
}
