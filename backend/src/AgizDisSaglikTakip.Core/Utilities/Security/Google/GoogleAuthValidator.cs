using Google.Apis.Auth;

namespace AgizDisSaglikTakip.Core.Utilities.Security.Google;

public class GoogleAuthValidator : IGoogleAuthValidator
{
    private readonly GoogleSettings _settings;

    public GoogleAuthValidator(GoogleSettings settings)
    {
        _settings = settings;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId }
            });

            return new GoogleUserInfo
            {
                Email = payload.Email,
                FullName = payload.Name,
                EmailVerified = payload.EmailVerified
            };
        }
        catch (InvalidJwtException)
        {
            // Token sahte, süresi geçmiş ya da bizim Client ID'mize ait değil.
            return null;
        }
    }
}
