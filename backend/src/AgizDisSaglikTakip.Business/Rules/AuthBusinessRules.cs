using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AgizDisSaglikTakip.Business.Rules;

public static class AuthBusinessRules
{
    // Bu desen basit olduğu için gerçekte "catastrophic backtracking" riski yok, ama SonarQube
    // her regex'e savunma amaçlı bir zaman aşımı konmasını istiyor (ReDoS'a karşı genel kural).
    private static readonly Regex PhoneRegex = new(@"^[0-9]{10,11}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));

    public static bool IsValidPhoneNumber(string phoneNumber) => PhoneRegex.IsMatch(phoneNumber);

    public static bool IsValidEmailFormat(string email)
    {
        // MailAddress, boş/whitespace metinle çağrılınca FormatException değil ArgumentException
        // fırlatıyor — bu erken kontrol olmadan boş e-postayla kayıt denemesi backend'i
        // yakalanmamış bir exception ile çökertirdi (bkz. birim testi).
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool IsValidPassword(string password)
    {
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        return true;
    }
}
