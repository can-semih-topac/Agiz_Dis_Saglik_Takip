using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.User;
using AgizDisSaglikTakip.Business.Rules;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.DataAccess.Abstract;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.Business.Concrete;

public class UserManager : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserManager> _logger;

    public UserManager(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IEmailService emailService,
        ILogger<UserManager> logger)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user == null)
            return ServiceResult<UserProfileDto>.Fail("Kullanıcı bulunamadı.");

        var profile = new UserProfileDto
        {
            Email = user.Email,
            FullName = user.FullName,
            BirthDate = user.BirthDate,
            PhoneNumber = user.PhoneNumber,
            HasPassword = !string.IsNullOrEmpty(user.PasswordEncrypted)
        };

        return ServiceResult<UserProfileDto>.Ok(profile);
    }

    public async Task<ServiceResult> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        if (!AuthBusinessRules.IsValidEmailFormat(dto.Email))
            return ServiceResult.Fail("Geçersiz e-posta formatı.");

        if (dto.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            return ServiceResult.Fail("Doğum tarihi gelecekte olamaz.");

        if (!AuthBusinessRules.IsValidPhoneNumber(dto.PhoneNumber))
            return ServiceResult.Fail("Telefon numarası 10 veya 11 haneli, sadece rakamlardan oluşmalı.");

        if (!string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var ownerOfEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (ownerOfEmail != null && ownerOfEmail.Id != userId)
                return ServiceResult.Fail("Bu e-posta adresi başka bir kullanıcıya ait.");
        }

        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.BirthDate = dto.BirthDate;
        user.PhoneNumber = dto.PhoneNumber;

        await _userRepository.UpdateAsync(user);

        return ServiceResult.Ok("Profil güncellendi.");
    }

    // Profil sayfasındaki "Şifreyi Değiştir" bölümü — UpdateProfileAsync'ten ayrı tutuluyor
    // çünkü burada mevcut parolanın doğrulanması gerekiyor (profildeki diğer alanlar için gerekmiyor).
    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        // Google ile oluşturulmuş hesaplarda henüz parola yok — bu durumda eski parola istemeden
        // doğrudan yeni parolayı kaydediyoruz (ilk kez parola belirleme).
        var hasExistingPassword = !string.IsNullOrEmpty(user.PasswordEncrypted);

        if (hasExistingPassword)
        {
            var currentPassword = _encryptionService.Decrypt(user.PasswordEncrypted!);
            if (currentPassword != dto.OldPassword)
                return ServiceResult.Fail("Mevcut parola yanlış.");
        }

        if (!AuthBusinessRules.IsValidPassword(dto.NewPassword))
            return ServiceResult.Fail("Parola en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.NewPassword != dto.NewPasswordConfirm)
            return ServiceResult.Fail("Parolalar eşleşmiyor.");

        user.PasswordEncrypted = _encryptionService.Encrypt(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        try
        {
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Parolanız Değiştirildi",
                AuthEmailTemplates.PasswordChangedEmail(user.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parola değişikliği bildirim maili gönderilemedi. Kullanıcı: {Email}", user.Email);
        }

        return ServiceResult.Ok("Parola güncellendi.");
    }
}
