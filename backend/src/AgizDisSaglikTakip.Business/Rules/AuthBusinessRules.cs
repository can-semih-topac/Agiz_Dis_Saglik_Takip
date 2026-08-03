using System.Net.Mail;

namespace AgizDisSaglikTakip.Business.Rules;

public static class AuthBusinessRules
{
    public static bool IsValidEmailFormat(string email)
    {
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
