using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AgizDisSaglikTakip.Business.Rules;

public static class AuthBusinessRules
{
    private static readonly Regex PhoneRegex = new(@"^[0-9]{10,11}$", RegexOptions.Compiled);

    public static bool IsValidPhoneNumber(string phoneNumber) => PhoneRegex.IsMatch(phoneNumber);

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
