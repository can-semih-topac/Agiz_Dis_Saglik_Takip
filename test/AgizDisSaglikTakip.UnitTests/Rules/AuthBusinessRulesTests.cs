using AgizDisSaglikTakip.Business.Rules;

namespace AgizDisSaglikTakip.UnitTests.Rules;

public class AuthBusinessRulesTests
{
    [Theory]
    [InlineData("can@gmail.com")]
    [InlineData("can.semih.topac18@gmail.com")]
    [InlineData("a@b.co")]
    public void IsValidEmailFormat_GecerliAdreslerdeTrueDoner(string email)
    {
        Assert.True(AuthBusinessRules.IsValidEmailFormat(email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("gecersiz")]
    [InlineData("gecersiz@")]
    [InlineData("@gmail.com")]
    [InlineData("bosluk var@gmail.com")]
    public void IsValidEmailFormat_GecersizAdreslerdeFalseDoner(string email)
    {
        Assert.False(AuthBusinessRules.IsValidEmailFormat(email));
    }

    [Theory]
    [InlineData("5551234567")]      // 10 hane
    [InlineData("05551234567")]     // 11 hane
    public void IsValidPhoneNumber_GecerliNumaralardaTrueDoner(string phone)
    {
        Assert.True(AuthBusinessRules.IsValidPhoneNumber(phone));
    }

    [Theory]
    [InlineData("123")]                // cok kisa
    [InlineData("555123456789")]       // cok uzun (12 hane)
    [InlineData("555-123-4567")]       // rakam disi karakter
    [InlineData("")]
    public void IsValidPhoneNumber_GecersizNumaralardaFalseDoner(string phone)
    {
        Assert.False(AuthBusinessRules.IsValidPhoneNumber(phone));
    }

    [Theory]
    [InlineData("Sifre123")]
    [InlineData("Aa1aaaaa")]
    [InlineData("ABCabc123XYZ")]
    public void IsValidPassword_KurallaraUyanSifrelerdeTrueDoner(string password)
    {
        Assert.True(AuthBusinessRules.IsValidPassword(password));
    }

    [Theory]
    [InlineData("Aa1aaaa")]      // 7 karakter - cok kisa
    [InlineData("sifre123")]     // buyuk harf yok
    [InlineData("SIFRE123")]     // kucuk harf yok
    [InlineData("SifreSifre")]   // rakam yok
    public void IsValidPassword_KurallaraUymayanSifrelerdeFalseDoner(string password)
    {
        Assert.False(AuthBusinessRules.IsValidPassword(password));
    }
}
