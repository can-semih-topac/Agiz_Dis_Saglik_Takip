namespace AgizDisSaglikTakip.Core.Utilities.Security.Jwt;

public interface ITokenService
{
    string CreateToken(int userId, string email, string role);

    // Yüksek entropili, rastgele bir refresh token üretir (JWT değil, düz metin).
    string GenerateRefreshToken();

    // Refresh token'ı DB'de saklamadan önce hash'lemek için — bkz. RefreshToken.TokenHash.
    string HashRefreshToken(string refreshToken);
}
