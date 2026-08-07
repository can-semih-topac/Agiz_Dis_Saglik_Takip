namespace AgizDisSaglikTakip.Business.Concrete;

// AuthManager ve UserManager (profilden şifre değiştirme) ortak kullanıyor.
internal static class AuthEmailTemplates
{
    public static string WelcomeEmail(string fullName) => $"""
        <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Hoş geldin, {fullName}!</h2>
                <p>Ağız ve Diş Sağlığı Takip Uygulaması'na kaydın başarıyla oluşturuldu.</p>
                <p>Artık hedeflerini belirleyip günlük alışkanlıklarını takip edebilirsin.</p>
            </body>
        </html>
        """;

    public static string ResetCodeEmail(string fullName, string code) => $"""
        <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Merhaba, {fullName}</h2>
                <p>Şifre sıfırlama isteğinde bulundunuz. Doğrulama kodunuz:</p>
                <p style="font-size: 28px; font-weight: bold; letter-spacing: 4px;">{code}</p>
                <p>Bu kod 10 dakika süreyle geçerlidir. Bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>
            </body>
        </html>
        """;

    public static string PasswordChangedEmail(string fullName) => $"""
        <html>
            <body style="font-family: Arial, sans-serif;">
                <h2>Merhaba, {fullName}</h2>
                <p>Hesabınızın şifresi az önce değiştirildi.</p>
                <p>Bu işlemi siz yapmadıysanız, lütfen en kısa sürede
                   <a href="mailto:can.semih.topac18@gmail.com">can.semih.topac18@gmail.com</a>
                   adresinden bizimle iletişime geçin.</p>
            </body>
        </html>
        """;
}
