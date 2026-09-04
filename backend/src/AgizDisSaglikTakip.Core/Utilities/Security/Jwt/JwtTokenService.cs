using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AgizDisSaglikTakip.Core.Utilities.Security.Jwt;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public string CreateToken(int userId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // RandomNumberGenerator: Random.Shared'ın aksine kriptografik olarak güvenli — tahmin
    // edilemez olması gereken bir oturum anahtarı ürettiğimiz için burada şart.
    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    // SHA-256 yeterli: parola gibi düşük entropili bir değer değil, zaten 512 bit rastgele
    // bir token'ı hash'liyoruz — brute-force'a karşı yavaş hash'lemeye (BCrypt vb.) gerek yok,
    // asıl istediğimiz DB sızsa bile ham token'ın ele geçmemesi.
    public string HashRefreshToken(string refreshToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hashBytes);
    }
}
