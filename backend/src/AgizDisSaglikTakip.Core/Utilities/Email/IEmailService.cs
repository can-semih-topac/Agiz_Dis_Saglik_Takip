namespace AgizDisSaglikTakip.Core.Utilities.Email;

public interface IEmailService
{
    Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody);
}
