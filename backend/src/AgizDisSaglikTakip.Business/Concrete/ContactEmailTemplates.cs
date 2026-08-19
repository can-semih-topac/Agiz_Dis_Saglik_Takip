namespace AgizDisSaglikTakip.Business.Concrete;

internal static class ContactEmailTemplates
{
    public static string NewMessageEmail(string fullName, string email, string message) => $"""
        <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Yeni Geri Bildirim</h2>
                <p><strong>Ad Soyad:</strong> {fullName}</p>
                <p><strong>E-posta:</strong> {email}</p>
                <p><strong>Mesaj:</strong></p>
                <p style="white-space: pre-wrap;">{message}</p>
            </body>
        </html>
        """;
}
