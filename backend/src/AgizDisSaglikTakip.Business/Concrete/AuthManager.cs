using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Business.Rules;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.Core.Utilities.Security.Google;
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
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IGoogleAuthValidator _googleAuthValidator;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthManager> _logger;

    public AuthManager(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        ITokenService tokenService,
        IEmailService emailService,
        IGoogleAuthValidator googleAuthValidator,
        IDistributedCache cache,
        ILogger<AuthManager> logger)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
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
            PasswordEncrypted = _encryptionService.Encrypt(dto.Password),
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
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult<LoginResultDto>.Fail("Kullanıcı bulunamadı.");

        // Google ile oluşturulmuş ve henüz şifre belirlememiş hesaplarda PasswordEncrypted boştur.
        if (string.IsNullOrEmpty(user.PasswordEncrypted))
            return ServiceResult<LoginResultDto>.Fail("Bu hesap Google ile oluşturulmuş. Google ile giriş yapabilir ya da 'Şifremi Unuttum' ile bir şifre belirleyebilirsiniz.");

        var decryptedPassword = _encryptionService.Decrypt(user.PasswordEncrypted);
        if (decryptedPassword != dto.Password)
            return ServiceResult<LoginResultDto>.Fail("Şifre yanlış.");

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
                PasswordEncrypted = null,
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
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        var code = Random.Shared.Next(100000, 1000000).ToString();
        await _cache.SetStringAsync(
            ResetCodeCacheKey(email),
            code,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

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
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        if (!await IsResetCodeValidAsync(dto.Email, dto.Code))
            return ServiceResult.Fail("Kod hatalı ya da süresi dolmuş.");

        return ServiceResult.Ok("Kod doğrulandı.");
    }

    // Adım 3:
    // Kod BURADA da tekrar kontrol ediliyor — Adım 2'yi atlayıp doğrudan bu endpoint'e istek atılsa bile kodsuz/yanlış kodla şifre değiştirilemesin diye.
    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        if (!await IsResetCodeValidAsync(dto.Email, dto.Code))
            return ServiceResult.Fail("Kod hatalı ya da süresi dolmuş.");

        if (!AuthBusinessRules.IsValidPassword(dto.NewPassword))
            return ServiceResult.Fail("Şifre en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.NewPassword != dto.NewPasswordConfirm)
            return ServiceResult.Fail("Şifreler eşleşmiyor.");

        user.PasswordEncrypted = _encryptionService.Encrypt(dto.NewPassword);
        await _userRepository.UpdateAsync(user);
        // Kod tek kullanımlık — başarılı sıfırlamadan sonra Redis'ten siliyoruz.
        await _cache.RemoveAsync(ResetCodeCacheKey(dto.Email));

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

    // Kodun geçerliliğini Redis'ten kontrol eder — süresi dolmuşsa Redis anahtarı zaten
    // kendiliğinden silinmiş olur, GetStringAsync null döner, elle süre kontrolü gerekmez.
    private async Task<bool> IsResetCodeValidAsync(string email, string code)
    {
        var storedCode = await _cache.GetStringAsync(ResetCodeCacheKey(email));
        return storedCode != null && storedCode == code;
    }

    private static string ResetCodeCacheKey(string email) => $"password-reset:{email}";
}
