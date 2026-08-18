using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Contact;
using AgizDisSaglikTakip.Business.Rules;
using AgizDisSaglikTakip.Core.Utilities.Email;
using AgizDisSaglikTakip.Core.Utilities.FileStorage;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.Business.Concrete;

public class ContactManager : IContactService
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IContactFileStorageService _fileStorageService;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IUserRepository _userRepository;
    private readonly IAdminActionLogService _adminActionLogService;
    private readonly ILogger<ContactManager> _logger;

    public ContactManager(
        IContactMessageRepository contactMessageRepository,
        IContactFileStorageService fileStorageService,
        IEmailService emailService,
        EmailSettings emailSettings,
        IUserRepository userRepository,
        IAdminActionLogService adminActionLogService,
        ILogger<ContactManager> logger)
    {
        _contactMessageRepository = contactMessageRepository;
        _fileStorageService = fileStorageService;
        _emailService = emailService;
        _emailSettings = emailSettings;
        _userRepository = userRepository;
        _adminActionLogService = adminActionLogService;
        _logger = logger;
    }

    public async Task<ServiceResult> SendMessageAsync(SendContactMessageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResult.Fail("Ad Soyad zorunlu.");

        if (!AuthBusinessRules.IsValidEmailFormat(dto.Email))
            return ServiceResult.Fail("Geçerli bir e-posta gir.");

        if (string.IsNullOrWhiteSpace(dto.Message))
            return ServiceResult.Fail("Mesaj boş olamaz.");

        string? imagePath = null;
        string? imageFileName = null;

        if (dto.ImageBytes != null && dto.ImageExtension != null)
        {
            var extension = dto.ImageExtension.ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return ServiceResult.Fail("Sadece .jpg, .jpeg, .png uzantılı görseller yüklenebilir.");

            using var stream = new MemoryStream(dto.ImageBytes);
            imagePath = await _fileStorageService.SaveFileAsync(stream, extension);
            imageFileName = $"fotograf{extension}";
        }

        var contactMessage = new ContactMessage
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Message = dto.Message,
            ImagePath = imagePath,
            CreatedAt = DateTime.Now
        };

        await _contactMessageRepository.AddAsync(contactMessage);

        try
        {
            await _emailService.SendAsync(new EmailMessage
            {
                ToEmail = _emailSettings.SenderEmail,
                Subject = "Yeni Geri Bildirim - Ağız ve Diş Sağlığı Takip",
                HtmlBody = ContactEmailTemplates.NewMessageEmail(dto.FullName, dto.Email, dto.Message),
                ReplyToEmail = dto.Email,
                AttachmentFileName = imageFileName,
                AttachmentBytes = dto.ImageBytes
            });
        }
        catch (Exception ex)
        {
            // Mail gitmese bile mesaj veritabanına kaydedildiği için kayıp yaşanmıyor.
            _logger.LogError(ex, "Geri bildirim maili gönderilemedi. Gönderen: {Email}", dto.Email);
        }

        return ServiceResult.Ok("Mesajınız gönderildi, teşekkürler!");
    }

    // Admin paneli için — gönderen herkes görebilsin diye herhangi bir kullanıcı filtresi yok, hepsi listeleniyor.
    public async Task<ServiceResult<List<ContactMessageDto>>> GetAllMessagesAsync()
    {
        var messages = await _contactMessageRepository.GetAllAsync();

        var dtos = messages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Message = m.Message,
                ImagePath = m.ImagePath,
                Status = m.Status,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return ServiceResult<List<ContactMessageDto>>.Ok(dtos);
    }

    public async Task<ServiceResult> MarkAsReviewedAsync(int adminUserId, int messageId)
    {
        var message = await _contactMessageRepository.GetAsync(m => m.Id == messageId);
        if (message == null)
            return ServiceResult.Fail("Mesaj bulunamadı.");

        if (message.Status == ContactMessageStatus.Reviewed)
            return ServiceResult.Fail("Mesaj zaten incelendi olarak işaretlenmiş.");

        message.Status = ContactMessageStatus.Reviewed;
        await _contactMessageRepository.UpdateAsync(message);

        var admin = await _userRepository.GetAsync(u => u.Id == adminUserId);
        await _adminActionLogService.RecordAsync(admin?.Email ?? "?", "Bize Ulaşın Mesajı İncelendi", message.Email);

        return ServiceResult.Ok("Mesaj incelendi olarak işaretlendi.");
    }
}
