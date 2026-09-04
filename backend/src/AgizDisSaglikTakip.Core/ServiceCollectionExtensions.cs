using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.FileStorage;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.Core.Utilities.Security.Google;
using AgizDisSaglikTakip.Core.Utilities.Security.Hashing;
using AgizDisSaglikTakip.Core.Utilities.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgizDisSaglikTakip.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration, string webRootPath)
    {
        var jwtSettings = new JwtSettings
        {
            SecretKey = configuration["Jwt:SecretKey"]!,
            Issuer = configuration["Jwt:Issuer"]!,
            Audience = configuration["Jwt:Audience"]!,
            ExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"]!)
        };
        services.AddSingleton(jwtSettings);
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        // Artık şifreler için kullanılmıyor (bkz. IPasswordHasher) — sadece LegacyPasswordMigrator'ın
        // native SQL Server'daki (güvenlik ağı) hâlâ eski formattaki kopyayı, oradan bir gün geri
        // yüklenirse otomatik taşıyabilmesi için kalıcı olarak burada tutuluyor.
        var aesKey = configuration["Encryption:AesKey"]!;
        services.AddSingleton<IEncryptionService>(new AesEncryptionService(aesKey));

        var emailSettings = new EmailSettings
        {
            SmtpHost = configuration["Email:SmtpHost"]!,
            SmtpPort = int.Parse(configuration["Email:SmtpPort"]!),
            SenderEmail = configuration["Email:SenderEmail"]!,
            SenderName = configuration["Email:SenderName"]!,
            SenderPassword = configuration["Email:SenderPassword"]!
        };
        services.AddSingleton(emailSettings);
        services.AddScoped<IEmailService, SmtpEmailService>();

        var googleSettings = new GoogleSettings
        {
            ClientId = configuration["Google:ClientId"]!
        };
        services.AddSingleton(googleSettings);
        services.AddSingleton<IGoogleAuthValidator, GoogleAuthValidator>();

        var fileStorageSettings = new FileStorageSettings
        {
            UploadFolderPath = Path.Combine(webRootPath, "uploads", "status-notes"),
            UploadUrlPath = "/uploads/status-notes"
        };
        services.AddSingleton(fileStorageSettings);
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        // Ayrı klasör (uploads/contact-messages) — status-note görselleriyle karışmasın diye
        // kendi FileStorageSettings'iyle doğrudan new'lenip kaydediliyor (DI'ın tek FileStorageSettings
        // singleton'ını ikinciyle çakıştırmamak için).
        var contactFileStorageSettings = new FileStorageSettings
        {
            UploadFolderPath = Path.Combine(webRootPath, "uploads", "contact-messages"),
            UploadUrlPath = "/uploads/contact-messages"
        };
        services.AddSingleton<IContactFileStorageService>(new ContactFileStorageService(contactFileStorageSettings));

        return services;
    }
}
