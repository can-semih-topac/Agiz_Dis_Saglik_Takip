using AgizDisSaglikTakip.Business.Concrete;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.Security.Google;
using AgizDisSaglikTakip.Core.Utilities.Security.Hashing;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Jwt;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.UnitTests.TestDoubles;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AgizDisSaglikTakip.UnitTests.Concrete;

// AuthManager'ın bağımlılıklarından bazıları (hash'leme, token üretimi) gerçek/hafif
// sınıflarla (Moq ile taklit yerine) kullanılıyor — bu sayede testler "mock her zaman
// istediğim değeri döner" yanılgısına düşmeden gerçek hash/rotasyon davranışını doğruluyor.
// IDistributedCache için de gerçek (ama bellek içi, Redis'siz) bir MemoryDistributedCache
// kullanılıyor — rate limit/refresh kodu mantığı Redis'in kendisine değil, IDistributedCache
// sözleşmesine dayandığı için bu tam bir davranış eşdeğeri sağlıyor.
public class AuthManagerTests
{
    private const string ValidPassword = "DogruSifre123!";

    private sealed record Fixture(
        AuthManager Manager,
        FakeUserRepository UserRepository,
        FakeRefreshTokenRepository RefreshTokenRepository,
        IDistributedCache Cache,
        IPasswordHasher PasswordHasher,
        ITokenService TokenService);

    private static Fixture CreateFixture(int maxLoginAttempts = 5, int refreshTokenExpirationDays = 7)
    {
        var userRepository = new FakeUserRepository();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var passwordHasher = new BCryptPasswordHasher();

        var jwtSettings = new JwtSettings
        {
            SecretKey = "birim-testleri-icin-yeterince-uzun-bir-gizli-anahtar-1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 15,
            RefreshTokenExpirationDays = refreshTokenExpirationDays
        };
        var tokenService = new JwtTokenService(jwtSettings);

        var manager = new AuthManager(
            userRepository,
            refreshTokenRepository,
            passwordHasher,
            tokenService,
            jwtSettings,
            Mock.Of<IEmailService>(),
            Mock.Of<IGoogleAuthValidator>(),
            cache,
            Mock.Of<ILogger<AuthManager>>());

        return new Fixture(manager, userRepository, refreshTokenRepository, cache, passwordHasher, tokenService);
    }

    private static User SeedUser(FakeUserRepository repo, string email, IPasswordHasher hasher, string password = ValidPassword)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            FullName = "Test Kullanici",
            PhoneNumber = "5551234567",
            CreatedAt = DateTime.UtcNow
        };
        repo.Seed(user);
        return user;
    }

    [Fact]
    public async Task LoginAsync_KullaniciYokIken_BasarisizDoner()
    {
        var f = CreateFixture();

        var result = await f.Manager.LoginAsync(new LoginDto { Email = "yok@ornek.local", Password = "x" });

        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginAsync_GoogleHesabindaSifreYokken_OzelMesajDoner()
    {
        var f = CreateFixture();
        f.UserRepository.Seed(new User { Email = "google@ornek.local", PasswordHash = null, FullName = "G", PhoneNumber = "5551234567" });

        var result = await f.Manager.LoginAsync(new LoginDto { Email = "google@ornek.local", Password = "herhangi" });

        Assert.False(result.Success);
        Assert.Contains("Google", result.Message);
    }

    [Fact]
    public async Task LoginAsync_DogruSifre_TokenVeRefreshTokenDoner()
    {
        var f = CreateFixture();
        SeedUser(f.UserRepository, "dogru@ornek.local", f.PasswordHasher);

        var result = await f.Manager.LoginAsync(new LoginDto { Email = "dogru@ornek.local", Password = ValidPassword });

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.Data!.Token));
        Assert.False(string.IsNullOrEmpty(result.Data.RefreshToken));
    }

    [Fact]
    public async Task LoginAsync_YanlisSifre_KalanDenemeHakkiIleBasarisizDoner()
    {
        var f = CreateFixture();
        SeedUser(f.UserRepository, "yanlis@ornek.local", f.PasswordHasher);

        var result = await f.Manager.LoginAsync(new LoginDto { Email = "yanlis@ornek.local", Password = "YanlisSifre1!" });

        Assert.False(result.Success);
        Assert.Contains("Kalan deneme hakkı: 4", result.Message);
    }

    [Fact]
    public async Task LoginAsync_BesYanlisDenemeSonrasi_HesapKilitlenirVeDogruSifreyleBileGirilemez()
    {
        var f = CreateFixture();
        SeedUser(f.UserRepository, "kilit@ornek.local", f.PasswordHasher);
        var dto = new LoginDto { Email = "kilit@ornek.local", Password = "YanlisSifre1!" };

        ServiceResult<LoginResultDto>? sonSonuc = null;
        for (var i = 0; i < 5; i++)
            sonSonuc = await f.Manager.LoginAsync(dto);

        Assert.False(sonSonuc!.Success);
        Assert.Contains("kilitlendi", sonSonuc.Message);

        // Kilitliyken DOĞRU şifreyle bile giriş denemesi başarısız kalmalı.
        var dogruSifreDenemesi = await f.Manager.LoginAsync(new LoginDto { Email = "kilit@ornek.local", Password = ValidPassword });
        Assert.False(dogruSifreDenemesi.Success);
        Assert.Contains("kilitli", dogruSifreDenemesi.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_GecerliToken_YeniTokenUretirVeEskisiniIptalEder()
    {
        var f = CreateFixture();
        var user = SeedUser(f.UserRepository, "refresh@ornek.local", f.PasswordHasher);
        var login = await f.Manager.LoginAsync(new LoginDto { Email = user.Email, Password = ValidPassword });
        var eskiRefreshToken = login.Data!.RefreshToken;

        var result = await f.Manager.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = eskiRefreshToken });

        Assert.True(result.Success);
        Assert.NotEqual(eskiRefreshToken, result.Data!.RefreshToken);

        // Eski token artık iptal edilmiş olmalı.
        var eskiKayit = f.RefreshTokenRepository.All.Single(t => t.TokenHash == f.TokenService.HashRefreshToken(eskiRefreshToken));
        Assert.NotNull(eskiKayit.RevokedAt);
    }

    [Fact]
    public async Task RefreshTokenAsync_IptalEdilmisTokenTekrarKullanilirsa_TumOturumlarKapatilir()
    {
        var f = CreateFixture();
        var user = SeedUser(f.UserRepository, "calinti@ornek.local", f.PasswordHasher);
        var login = await f.Manager.LoginAsync(new LoginDto { Email = user.Email, Password = ValidPassword });
        var ilkRefreshToken = login.Data!.RefreshToken;

        // Normal rotasyon: ilk token bir kez kullanılıp yenisiyle değiştiriliyor.
        var ilkYenileme = await f.Manager.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = ilkRefreshToken });
        Assert.True(ilkYenileme.Success);
        var ikinciRefreshToken = ilkYenileme.Data!.RefreshToken;

        // Çalıntı senaryosu: iptal edilmiş İLK token tekrar kullanılmaya çalışılıyor.
        var calintiDenemesi = await f.Manager.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = ilkRefreshToken });
        Assert.False(calintiDenemesi.Success);

        // Güvenlik önlemi: bu arada üretilmiş, HALA GEÇERLİ olması gereken ikinci token da iptal edilmiş olmalı.
        var ikinciTokenDenemesi = await f.Manager.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = ikinciRefreshToken });
        Assert.False(ikinciTokenDenemesi.Success);
    }

    [Fact]
    public async Task RequestPasswordResetCodeAsync_KullaniciVarsa_CacheeKoduYazar()
    {
        var f = CreateFixture();
        SeedUser(f.UserRepository, "kod@ornek.local", f.PasswordHasher);

        var result = await f.Manager.RequestPasswordResetCodeAsync("kod@ornek.local");

        Assert.True(result.Success);
        var kod = await f.Cache.GetStringAsync("password-reset:kod@ornek.local");
        Assert.NotNull(kod);
        Assert.Equal(6, kod!.Length);
    }

    [Fact]
    public async Task VerifyPasswordResetCodeAsync_DogruKod_BasariliDoner()
    {
        var f = CreateFixture();
        SeedUser(f.UserRepository, "dogrukod@ornek.local", f.PasswordHasher);
        await f.Manager.RequestPasswordResetCodeAsync("dogrukod@ornek.local");
        var kod = await f.Cache.GetStringAsync("password-reset:dogrukod@ornek.local");

        var result = await f.Manager.VerifyPasswordResetCodeAsync(
            new VerifyResetCodeDto { Email = "dogrukod@ornek.local", Code = kod! });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task VerifyPasswordResetCodeAsync_BesYanlisDeneme_KoduGecersizKilar()
    {
        var f = CreateFixture();
        SeedUser(f.UserRepository, "yanliskod@ornek.local", f.PasswordHasher);
        await f.Manager.RequestPasswordResetCodeAsync("yanliskod@ornek.local");

        ServiceResult? sonSonuc = null;
        for (var i = 0; i < 5; i++)
            sonSonuc = await f.Manager.VerifyPasswordResetCodeAsync(
                new VerifyResetCodeDto { Email = "yanliskod@ornek.local", Code = "000000" });

        Assert.False(sonSonuc!.Success);
        Assert.Contains("Çok fazla", sonSonuc.Message);

        // Kod artık tamamen geçersiz kılınmış olmalı (Redis'ten silinmiş).
        var kalanKod = await f.Cache.GetStringAsync("password-reset:yanliskod@ornek.local");
        Assert.Null(kalanKod);
    }

    [Fact]
    public async Task ResetPasswordAsync_BasariliSifirlama_KilitliHesabinKilidiniKaldirir()
    {
        var f = CreateFixture();
        var user = SeedUser(f.UserRepository, "kilitac@ornek.local", f.PasswordHasher);

        // Hesabı 5 yanlış denemeyle kilitliyoruz.
        for (var i = 0; i < 5; i++)
            await f.Manager.LoginAsync(new LoginDto { Email = user.Email, Password = "Yanlis123!" });

        await f.Manager.RequestPasswordResetCodeAsync(user.Email);
        var kod = await f.Cache.GetStringAsync($"password-reset:{user.Email}");

        var resetResult = await f.Manager.ResetPasswordAsync(new ResetPasswordDto
        {
            Email = user.Email,
            Code = kod!,
            NewPassword = "YeniSifre456!",
            NewPasswordConfirm = "YeniSifre456!"
        });
        Assert.True(resetResult.Success);

        // Kilit kalksın diye: yeni şifreyle giriş hemen başarılı olmalı (15 dk beklemeden).
        var girisResult = await f.Manager.LoginAsync(new LoginDto { Email = user.Email, Password = "YeniSifre456!" });
        Assert.True(girisResult.Success);
    }

    [Fact]
    public async Task LogoutAsync_RefreshTokenIptalEdilirVeTekrarKullanilamaz()
    {
        var f = CreateFixture();
        var user = SeedUser(f.UserRepository, "cikis@ornek.local", f.PasswordHasher);
        var login = await f.Manager.LoginAsync(new LoginDto { Email = user.Email, Password = ValidPassword });

        var logoutResult = await f.Manager.LogoutAsync(new RefreshTokenDto { RefreshToken = login.Data!.RefreshToken });
        Assert.True(logoutResult.Success);

        var refreshDenemesi = await f.Manager.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = login.Data.RefreshToken });
        Assert.False(refreshDenemesi.Success);
    }
}
