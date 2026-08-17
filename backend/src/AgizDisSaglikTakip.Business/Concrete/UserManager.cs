using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.User;
using AgizDisSaglikTakip.Business.Rules;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.Core.Utilities.Security.Encryption;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.Business.Concrete;

public class UserManager : IUserService
{
    // Proje henüz kendi alan adına sahip olmadığı için davet e-postalarında bu adrese yönlendiriyoruz.
    private const string LoginLink = "http://localhost:4200/login";

    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailService _emailService;
    private readonly IWillpowerService _willpowerService;
    private readonly ILogger<UserManager> _logger;

    public UserManager(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IEmailService emailService,
        IWillpowerService willpowerService,
        ILogger<UserManager> logger)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _emailService = emailService;
        _willpowerService = willpowerService;
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
            HasPassword = !string.IsNullOrEmpty(user.PasswordEncrypted),
            MustChangePassword = user.MustChangePassword
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

        if (dto.BirthDate.HasValue && dto.BirthDate.Value > DateOnly.FromDateTime(DateTime.Today))
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
    // çünkü burada mevcut şifrenin doğrulanması gerekiyor (profildeki diğer alanlar için gerekmiyor).
    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        // Google ile oluşturulmuş hesaplarda henüz şifre yok — bu durumda eski şifre istemeden
        // doğrudan yeni şifreyi kaydediyoruz (ilk kez şifre belirleme).
        var hasExistingPassword = !string.IsNullOrEmpty(user.PasswordEncrypted);

        if (hasExistingPassword)
        {
            var currentPassword = _encryptionService.Decrypt(user.PasswordEncrypted!);
            if (currentPassword != dto.OldPassword)
                return ServiceResult.Fail("Mevcut şifre yanlış.");
        }

        if (!AuthBusinessRules.IsValidPassword(dto.NewPassword))
            return ServiceResult.Fail("Şifre en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

        if (dto.NewPassword != dto.NewPasswordConfirm)
            return ServiceResult.Fail("Şifreler eşleşmiyor.");

        user.PasswordEncrypted = _encryptionService.Encrypt(dto.NewPassword);
        // Admin panelinden geçici şifreyle oluşturulmuşsa, ilk (gerçek) şifre değişikliğiyle bu uyarı kalkar.
        user.MustChangePassword = false;
        await _userRepository.UpdateAsync(user);

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

    // Kalıcı (hard) silme — yumuşak silme ileride eklenecek.
    public async Task<ServiceResult> DeleteAccountAsync(int userId)
    {
        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        await _userRepository.DeleteAsync(user);

        return ServiceResult.Ok("Hesap silindi.");
    }

    // Admin paneli için — en yeni kayıt üstte.
    public async Task<ServiceResult<List<UserAdminDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        var dtos = new List<UserAdminDto>();
        foreach (var u in users.OrderByDescending(u => u.CreatedAt))
        {
            var scoreResult = await _willpowerService.GetScoreAsync(u.Id);

            dtos.Add(new UserAdminDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                BirthDate = u.BirthDate,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                WillpowerScore = scoreResult.Success ? scoreResult.Data!.Score : 0
            });
        }

        return ServiceResult<List<UserAdminDto>>.Ok(dtos);
    }

    // Admin panelinden yeni kullanıcı/admin ekleme. Admin -> geçici şifre zorunlu, ilk girişte
    // değiştirmesi hatırlatılır. User -> şifresiz oluşturulur (Google hesabıyla aynı mantık),
    // e-postasına giriş sayfasının linkiyle bir davet gönderilir.
    public async Task<ServiceResult> CreateUserByAdminAsync(CreateUserByAdminDto dto)
    {
        if (!AuthBusinessRules.IsValidEmailFormat(dto.Email))
            return ServiceResult.Fail("Geçersiz e-posta formatı.");

        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return ServiceResult.Fail("Bu e-posta adresi zaten kayıtlı.");

        var user = new User
        {
            Email = dto.Email,
            Role = dto.Role,
            FullName = string.Empty,
            PhoneNumber = string.Empty,
            BirthDate = null,
            CreatedAt = DateTime.Now
        };

        if (dto.Role == Role.Admin)
        {
            if (string.IsNullOrEmpty(dto.TemporaryPassword) || !AuthBusinessRules.IsValidPassword(dto.TemporaryPassword))
                return ServiceResult.Fail("Geçici şifre en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermeli.");

            user.PasswordEncrypted = _encryptionService.Encrypt(dto.TemporaryPassword);
            user.MustChangePassword = true;

            await _userRepository.AddAsync(user);

            return ServiceResult.Ok("Admin hesabı oluşturuldu.");
        }

        user.PasswordEncrypted = null;
        await _userRepository.AddAsync(user);

        try
        {
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Davet Edildiniz - Ağız ve Diş Sağlığı Takip",
                AuthEmailTemplates.InviteEmail(LoginLink));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Davet maili gönderilemedi. Kullanıcı: {Email}", user.Email);
        }

        return ServiceResult.Ok("Kullanıcı oluşturuldu, davet e-postası gönderildi.");
    }

    // Kalıcı (hard) silme — DeleteAccountAsync'teki gibi, yumuşak silme ileride eklenecek.
    // Onay ekranı frontend'de; burada sadece adminin kendi hesabını silmesini engelliyoruz.
    public async Task<ServiceResult> DeleteUserByAdminAsync(int adminUserId, int targetUserId)
    {
        if (adminUserId == targetUserId)
            return ServiceResult.Fail("Kendi hesabınızı buradan silemezsiniz.");

        var user = await _userRepository.GetAsync(u => u.Id == targetUserId);
        if (user == null)
            return ServiceResult.Fail("Kullanıcı bulunamadı.");

        await _userRepository.DeleteAsync(user);

        return ServiceResult.Ok("Kullanıcı silindi.");
    }
}
