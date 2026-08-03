using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.Core.Utilities.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgizDisSaglikTakip.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }
}
