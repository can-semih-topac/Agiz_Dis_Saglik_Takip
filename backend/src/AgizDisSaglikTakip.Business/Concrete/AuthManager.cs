using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Business.Rules;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.Core.Utilities.Security.Jwt;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.Business.Concrete;

public class AuthManager : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthManager> _logger;

    public AuthManager(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<AuthManager> logger)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterDto dto) //Kayıt olma
    {
        if (!AuthBusinessRules.IsValidEmailFormat(dto.Email))
            return ServiceResult.Fail("Geçersiz e-posta formatı.");

        if (!AuthBusinessRules.IsValidPassword(dto.Password))
            return ServiceResult.Fail("Parola en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.Password != dto.PasswordConfirm)
            return ServiceResult.Fail("Parolalar eşleşmiyor.");

        if (dto.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            return ServiceResult.Fail("Doğum tarihi gelecekte olamaz.");

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return ServiceResult.Fail("Bu e-posta adresi zaten kayıtlı.");

        var user = new User
        {
            Email = dto.Email,
            PasswordEncrypted = _encryptionService.Encrypt(dto.Password),
            FullName = dto.FullName,
            BirthDate = dto.BirthDate,
            CreatedAt = DateTime.Now
        };

        await _userRepository.AddAsync(user);

        try
        {
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Kaydınız Başarıyla Oluşturuldu",
                BuildWelcomeEmailHtml(user.FullName));
        }
        catch (Exception ex)
        {
            // Mail sunucusu geçici olarak erişilemez olsa bile kayıt işlemi geçerli kalmalı ama sebebi görebilmek için logluyoruz.
            _logger.LogError(ex, "Kayıt sonrası bilgilendirme maili gönderilemedi. Kullanıcı: {Email}", user.Email);
        }

        return ServiceResult.Ok("Kayıt başarılı.");
    }

    public async Task<ServiceResult<LoginResultDto>> LoginAsync(LoginDto dto) //Giriş yapma
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult<LoginResultDto>.Fail("Kullanıcı bulunamadı.");

        var decryptedPassword = _encryptionService.Decrypt(user.PasswordEncrypted);
        if (decryptedPassword != dto.Password)
            return ServiceResult<LoginResultDto>.Fail("Parola yanlış.");

        var token = _tokenService.CreateToken(user.Id, user.Email);

        var result = new LoginResultDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };

        return ServiceResult<LoginResultDto>.Ok(result, "Giriş başarılı.");
    }

    public async Task<ServiceResult> VerifyEmailForPasswordResetAsync(string email) //Şifre sıfırlama için e-posta doğrulama
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto) //Şifre sıfırlama 
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        if (!AuthBusinessRules.IsValidPassword(dto.NewPassword))
            return ServiceResult.Fail("Parola en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.NewPassword != dto.NewPasswordConfirm)
            return ServiceResult.Fail("Parolalar eşleşmiyor.");

        user.PasswordEncrypted = _encryptionService.Encrypt(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return ServiceResult.Ok("Parola güncellendi.");
    }

    private static string BuildWelcomeEmailHtml(string fullName) // Hoşgeldin maili oluşturmak için HTML şablonu
    {
        return $"""
            <html>
                <body style="font-family: Arial, sans-serif;">
                    <h2>Hoş geldin, {fullName}!</h2>
                    <p>Ağız ve Diş Sağlığı Takip Uygulaması'na kaydın başarıyla oluşturuldu.</p>
                    <p>Artık hedeflerini belirleyip günlük alışkanlıklarını takip edebilirsin.</p>
                </body>
            </html>
            """;
    }
}
