using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AgizDisSaglikTakip.Core.Utilities.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(EmailSettings settings)
    {
        _settings = settings;
    }

    public async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
    {
        await SendAsync(new EmailMessage { ToEmail = toEmail, Subject = subject, HtmlBody = htmlBody });
    }

    public async Task SendAsync(EmailMessage email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(email.ToEmail));
        if (!string.IsNullOrEmpty(email.ReplyToEmail))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(email.ReplyToEmail));
        }
        message.Subject = email.Subject;

        var builder = new BodyBuilder { HtmlBody = email.HtmlBody };
        if (email.AttachmentBytes != null && email.AttachmentFileName != null)
        {
            builder.Attachments.Add(email.AttachmentFileName, email.AttachmentBytes);
        }
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
