using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.Constants;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Business.Rules;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Google;
using AgizDisSaglikTakip.Core.Utilities.Security.Hashing;
using AgizDisSaglikTakip.Core.Utilities.Security.Jwt;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.Business.Concrete;

public class AuthManager : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IGoogleAuthValidator _googleAuthValidator;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthManager> _logger;

    // Giriş kilitleme: art arda yanlış şifreyle şifreyi zorla kırmaya çalışmayı (brute-force)
    // engellemek için. Eşiğe ulaşınca hesap bir süre kilitleniyor; kullanıcı isterse
    // beklemek yerine "Şifremi Unuttum" ile şifresini sıfırlayıp kilidi anında kaldırabilir
    // (bkz. ResetPasswordAsync).
    private const int MaxLoginAttempts = 5;
    private static readonly TimeSpan LoginLockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LoginFailWindow = TimeSpan.FromMinutes(15);

    // Şifre sıfırlama kodu deneme sınırı: 6 haneli kod 900.000 ihtimalden oluşuyor, deneme
    // sınırı olmadan 10 dakikalık geçerlilik süresi içinde otomatik denemeyle (brute-force)
    // kırılabilir. Eşiğe ulaşınca kod geçersiz kılınır, kullanıcı yeni kod istemek zorunda kalır.
    private const int MaxResetCodeAttempts = 5;

    public AuthManager(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IGoogleAuthValidator googleAuthValidator,
        IDistributedCache cache,
        ILogger<AuthManager> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _googleAuthValidator = googleAuthValidator;
        _cache = cache;
        _logger = logger;
    }

    //Kayıt olma
    public async Task<ServiceResult> RegisterAsync(RegisterDto dto) 
    {
        if (!AuthBusinessRules.IsValidEmailFormat(dto.Email))
            return ServiceResult.Fail("Geçersiz e-posta formatı.");

        if (!AuthBusinessRules.IsValidPassword(dto.Password))
            return ServiceResult.Fail("Şifre en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.Password != dto.PasswordConfirm)
            return ServiceResult.Fail("Şifreler eşleşmiyor.");

        if (dto.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            return ServiceResult.Fail("Doğum tarihi gelecekte olamaz.");

        if (!AuthBusinessRules.IsValidPhoneNumber(dto.PhoneNumber))
            return ServiceResult.Fail("Telefon numarası 10 veya 11 haneli, sadece rakamlardan oluşmalı.");

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return ServiceResult.Fail("Bu e-posta adresi zaten kayıtlı.");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            BirthDate = dto.BirthDate,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.Now
        };

        await _userRepository.AddAsync(user);

        try
        {
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Kaydınız Başarıyla Oluşturuldu",
                AuthEmailTemplates.WelcomeEmail(user.FullName));
        }
        catch (Exception ex)
        {
            // Mail sunucusu geçici olarak erişilemez olsa bile kayıt işlemi geçerli kalmalı ama sebebi görebilmek için logluyoruz.
            _logger.LogError(ex, "Kayıt sonrası bilgilendirme maili gönderilemedi. Kullanıcı: {Email}", user.Email);
        }

        return ServiceResult.Ok("Kayıt başarılı.");
    }

    //Giriş yapma
    public async Task<ServiceResult<LoginResultDto>> LoginAsync(LoginDto dto)
    {
        var lockoutRemaining = await GetLoginLockoutRemainingMinutesAsync(dto.Email);
        if (lockoutRemaining.HasValue)
            return ServiceResult<LoginResultDto>.Fail(
                $"Çok fazla başarısız giriş denemesi. Hesabınız {lockoutRemaining.Value} dakika daha kilitli. Hemen giriş yapmak isterseniz 'Şifremi Unuttum' ile şifrenizi sıfırlayabilirsiniz.");

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult<LoginResultDto>.Fail(ErrorMessages.UserNotFound);

        // Google ile oluşturulmuş ve henüz şifre belirlememiş hesaplarda PasswordHash boştur.
        if (string.IsNullOrEmpty(user.PasswordHash))
            return ServiceResult<LoginResultDto>.Fail("Bu hesap Google ile oluşturulmuş. Google ile giriş yapabilir ya da 'Şifremi Unuttum' ile bir şifre belirleyebilirsiniz.");

        if (!_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            var (justLocked, remainingAttempts) = await RegisterFailedLoginAttemptAsync(dto.Email);
            if (justLocked)
                return ServiceResult<LoginResultDto>.Fail(
                    $"Şifrenizi {MaxLoginAttempts} kez yanlış girdiniz. Hesabınız {LoginLockoutDuration.TotalMinutes:0} dakika kilitlendi. Hemen giriş yapmak isterseniz 'Şifremi Unuttum' ile şifrenizi sıfırlayabilirsiniz.");

            return ServiceResult<LoginResultDto>.Fail($"Şifre yanlış. Kalan deneme hakkı: {remainingAttempts}.");
        }

        await _cache.RemoveAsync(LoginFailCountCacheKey(dto.Email));

        var token = _tokenService.CreateToken(user.Id, user.Email, user.Role.ToString());

        var result = new LoginResultDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            IsAdmin = user.Role == Role.Admin
        };

        return ServiceResult<LoginResultDto>.Ok(result, "Giriş başarılı.");
    }

    // Google ile giriş: ID token doğrulanır, aynı e-postalı kullanıcı varsa ona giriş yapılır
    // (elle kayıt olmuş biri de olabilir — şifresine dokunulmaz), yoksa otomatik kayıt oluşturulur.
    public async Task<ServiceResult<LoginResultDto>> GoogleLoginAsync(GoogleLoginDto dto)
    {
        var googleUser = await _googleAuthValidator.ValidateAsync(dto.IdToken);
        if (googleUser == null)
            return ServiceResult<LoginResultDto>.Fail("Google doğrulaması başarısız.");

        if (!googleUser.EmailVerified)
            return ServiceResult<LoginResultDto>.Fail("Google hesabınızın e-postası doğrulanmamış.");

        var user = await _userRepository.GetByEmailAsync(googleUser.Email);

        if (user == null)
        {
            user = new User
            {
                Email = googleUser.Email,
                FullName = googleUser.FullName,
                PhoneNumber = string.Empty,
                BirthDate = null,
                PasswordHash = null,
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddAsync(user);

            try
            {
                await _emailService.SendHtmlEmailAsync(
                    user.Email,
                    "Kaydınız Başarıyla Oluşturuldu",
                    AuthEmailTemplates.WelcomeEmail(user.FullName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google kaydı sonrası bilgilendirme maili gönderilemedi. Kullanıcı: {Email}", user.Email);
            }
        }

        var token = _tokenService.CreateToken(user.Id, user.Email, user.Role.ToString());

        var result = new LoginResultDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            IsAdmin = user.Role == Role.Admin
        };

        return ServiceResult<LoginResultDto>.Ok(result, "Giriş başarılı.");
    }

    // Adım 1:
    // Email kayıtlıysa 6 haneli bir kod üretip 10 dakika geçerlilikle Redis'e yazıyoruz ve mailliyoruz.
    // Redis'e yazmamızın sebebi: bu kod tamamen geçici bir veri, 10 dakika sonra anlamsızlaşıyor —
    // SQL Server'da kalıcı bir kolonda tutup elle "süresi geçti mi" kontrolü yapmak yerine,
    // Redis'in TTL (time to live) özelliğiyle süre dolunca kendiliğinden silinmesini sağlıyoruz.
    public async Task<ServiceResult> RequestPasswordResetCodeAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return ServiceResult.Fail(ErrorMessages.UserNotFound);

        var code = Random.Shared.Next(100000, 1000000).ToString();
        await _cache.SetStringAsync(
            ResetCodeCacheKey(email),
            code,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        // Yeni kod = yeni bir deneme hakkı seti. Önceki kodun yanlış denemelerinden kalan sayaç sıfırlanır.
        await _cache.RemoveAsync(ResetAttemptsCacheKey(email));

        try
        {
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Şifre Sıfırlama Kodu",
                AuthEmailTemplates.ResetCodeEmail(user.FullName, code));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre sıfırlama kodu gönderilemedi. Kullanıcı: {Email}", user.Email);
            return ServiceResult.Fail("Kod gönderilemedi, lütfen daha sonra tekrar deneyin.");
        }

        return ServiceResult.Ok("Doğrulama kodu e-posta adresinize gönderildi.");
    }

    // Adım 2: 
    // Kod doğrulama (ResetPasswordAsync'te tekrar yapılıyor, bu adım sadece kullanıcı deneyimi için)
    public async Task<ServiceResult> VerifyPasswordResetCodeAsync(VerifyResetCodeDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult.Fail(ErrorMessages.UserNotFound);

        var checkResult = await CheckResetCodeAsync(dto.Email, dto.Code);
        if (checkResult == ResetCodeCheckResult.TooManyAttempts)
            return ServiceResult.Fail("Çok fazla yanlış deneme yapıldı. Lütfen yeni bir kod isteyin.");
        if (checkResult == ResetCodeCheckResult.Invalid)
            return ServiceResult.Fail("Kod hatalı ya da süresi dolmuş.");

        return ServiceResult.Ok("Kod doğrulandı.");
    }

    // Adım 3:
    // Kod BURADA da tekrar kontrol ediliyor — Adım 2'yi atlayıp doğrudan bu endpoint'e istek atılsa bile kodsuz/yanlış kodla şifre değiştirilemesin diye.
    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult.Fail(ErrorMessages.UserNotFound);

        var checkResult = await CheckResetCodeAsync(dto.Email, dto.Code);
        if (checkResult == ResetCodeCheckResult.TooManyAttempts)
            return ServiceResult.Fail("Çok fazla yanlış deneme yapıldı. Lütfen yeni bir kod isteyin.");
        if (checkResult == ResetCodeCheckResult.Invalid)
            return ServiceResult.Fail("Kod hatalı ya da süresi dolmuş.");

        if (!AuthBusinessRules.IsValidPassword(dto.NewPassword))
            return ServiceResult.Fail("Şifre en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.NewPassword != dto.NewPasswordConfirm)
            return ServiceResult.Fail("Şifreler eşleşmiyor.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        await _userRepository.UpdateAsync(user);
        // Kod tek kullanımlık — başarılı sıfırlamadan sonra Redis'ten siliyoruz.
        await _cache.RemoveAsync(ResetCodeCacheKey(dto.Email));
        // Şifresini sıfırlayan kullanıcı, giriş denemelerinden dolayı kilitli kalmış olsa bile
        // yeni şifresiyle hemen giriş yapabilmeli — kilidi burada da kaldırıyoruz.
        await _cache.RemoveAsync(LoginFailCountCacheKey(dto.Email));
        await _cache.RemoveAsync(LoginLockoutCacheKey(dto.Email));

        try
        {
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Şifreniz Değiştirildi",
                AuthEmailTemplates.PasswordChangedEmail(user.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şifre değişikliği bildirim maili gönderilemedi. Kullanıcı: {Email}", user.Email);
        }

        return ServiceResult.Ok("Şifre güncellendi.");
    }

    private enum ResetCodeCheckResult { Valid, Invalid, TooManyAttempts }

    // Kodun geçerliliğini Redis'ten kontrol eder — süresi dolmuşsa Redis anahtarı zaten
    // kendiliğinden silinmiş olur, GetStringAsync null döner, elle süre kontrolü gerekmez.
    // Yanlış her denemede sayaç artar; MaxResetCodeAttempts'e ulaşınca kod geçersiz kılınır
    // (Redis'ten silinir) ki 6 haneli kod otomatik denemeyle (brute-force) kırılamasın.
    private async Task<ResetCodeCheckResult> CheckResetCodeAsync(string email, string code)
    {
        var attemptsRaw = await _cache.GetStringAsync(ResetAttemptsCacheKey(email));
        var attempts = attemptsRaw == null ? 0 : int.Parse(attemptsRaw);
        if (attempts >= MaxResetCodeAttempts)
            return ResetCodeCheckResult.TooManyAttempts;

        var storedCode = await _cache.GetStringAsync(ResetCodeCacheKey(email));
        if (storedCode == null)
            return ResetCodeCheckResult.Invalid;

        if (storedCode == code)
        {
            await _cache.RemoveAsync(ResetAttemptsCacheKey(email));
            return ResetCodeCheckResult.Valid;
        }

        attempts++;
        if (attempts >= MaxResetCodeAttempts)
        {
            // Deneme hakkı bitti: kodu da geçersiz kılıyoruz, kullanıcı yeni kod istemek zorunda.
            await _cache.RemoveAsync(ResetCodeCacheKey(email));
            await _cache.RemoveAsync(ResetAttemptsCacheKey(email));
            return ResetCodeCheckResult.TooManyAttempts;
        }

        await _cache.SetStringAsync(
            ResetAttemptsCacheKey(email),
            attempts.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        return ResetCodeCheckResult.Invalid;
    }

    // Yanlış şifre denemesini sayar; eşiğe ulaşınca hesabı LoginLockoutDuration süreyle kilitler.
    // Döndürdüğü (justLocked, remainingAttempts): justLocked=true ise bu deneme hesabı AZ ÖNCE
    // kilitledi (LoginAsync farklı bir mesaj göstermek için kullanır); değilse kaç deneme hakkı kaldığını taşır.
    private async Task<(bool JustLocked, int RemainingAttempts)> RegisterFailedLoginAttemptAsync(string email)
    {
        var countRaw = await _cache.GetStringAsync(LoginFailCountCacheKey(email));
        var count = (countRaw == null ? 0 : int.Parse(countRaw)) + 1;

        if (count >= MaxLoginAttempts)
        {
            var lockoutEnd = DateTime.UtcNow.Add(LoginLockoutDuration);
            await _cache.SetStringAsync(
                LoginLockoutCacheKey(email),
                lockoutEnd.ToString("O"),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = LoginLockoutDuration });
            await _cache.RemoveAsync(LoginFailCountCacheKey(email));

            _logger.LogWarning(
                "Hesap kilitlendi: {Email} - {Count} basarisiz giris denemesi sonrasi {Dakika} dakika kilitlendi.",
                email, count, LoginLockoutDuration.TotalMinutes);

            return (true, 0);
        }

        await _cache.SetStringAsync(
            LoginFailCountCacheKey(email),
            count.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = LoginFailWindow });

        return (false, MaxLoginAttempts - count);
    }

    // Hesap kilitliyse kalan süreyi dakika olarak döner, değilse null.
    private async Task<int?> GetLoginLockoutRemainingMinutesAsync(string email)
    {
        var lockoutRaw = await _cache.GetStringAsync(LoginLockoutCacheKey(email));
        if (lockoutRaw == null)
            return null;

        if (!DateTime.TryParse(
                lockoutRaw,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var lockoutEnd))
            return null;

        var remaining = lockoutEnd - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
            return null;

        return (int)Math.Ceiling(remaining.TotalMinutes);
    }

    private static string ResetCodeCacheKey(string email) => $"password-reset:{email}";
    private static string ResetAttemptsCacheKey(string email) => $"password-reset-attempts:{email}";
    private static string LoginFailCountCacheKey(string email) => $"login-fail-count:{email}";
    private static string LoginLockoutCacheKey(string email) => $"login-lockout:{email}";
}
