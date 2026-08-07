namespace AgizDisSaglikTakip.Core.Utilities.Security.Google;

public interface IGoogleAuthValidator
{
    // ID token geçersiz/sahteyse null döner — geçerliyse Google'ın doğruladığı kullanıcı bilgilerini verir.
    Task<GoogleUserInfo?> ValidateAsync(string idToken);
}
