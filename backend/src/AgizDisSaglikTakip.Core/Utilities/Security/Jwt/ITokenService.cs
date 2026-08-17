namespace AgizDisSaglikTakip.Core.Utilities.Security.Jwt;

public interface ITokenService
{
    string CreateToken(int userId, string email, string role);
}
