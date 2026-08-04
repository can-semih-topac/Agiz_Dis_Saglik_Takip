using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.FileStorage;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
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

        var fileStorageSettings = new FileStorageSettings
        {
            UploadFolderPath = Path.Combine(webRootPath, "uploads", "status-notes"),
            UploadUrlPath = "/uploads/status-notes"
        };
        services.AddSingleton(fileStorageSettings);
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
