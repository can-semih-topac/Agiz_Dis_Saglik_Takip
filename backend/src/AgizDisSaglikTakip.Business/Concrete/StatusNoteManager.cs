using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.StatusNote;
using AgizDisSaglikTakip.Core.Utilities.FileStorage;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.Business.Concrete;

public class StatusNoteManager : IStatusNoteService
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

    private readonly IStatusNoteRepository _statusNoteRepository;
    private readonly IFileStorageService _fileStorageService;

    public StatusNoteManager(IStatusNoteRepository statusNoteRepository, IFileStorageService fileStorageService)
    {
        _statusNoteRepository = statusNoteRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<ServiceResult> CreateStatusNoteAsync(int userId, CreateStatusNoteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
            return ServiceResult.Fail("Açıklama boş olamaz.");

        string? imagePath = null;

        if (dto.ImageStream != null && dto.ImageExtension != null)
        {
            var extension = dto.ImageExtension.ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return ServiceResult.Fail("Sadece .jpg, .jpeg, .png uzantılı görseller yüklenebilir.");

            imagePath = await _fileStorageService.SaveFileAsync(dto.ImageStream, extension);
        }

        var statusNote = new StatusNote
        {
            UserId = userId,
            Description = dto.Description,
            ImagePath = imagePath,
            CreatedAt = DateTime.Now
        };

        await _statusNoteRepository.AddAsync(statusNote);

        return ServiceResult.Ok("Not kaydedildi.");
    }

    public async Task<ServiceResult<List<StatusNoteDto>>> GetLast7DaysAsync(int userId)
    {
        var records = await _statusNoteRepository.GetLast7DaysByUserIdAsync(userId);

        var dtos = records.Select(sn => new StatusNoteDto
        {
            Id = sn.Id,
            Description = sn.Description,
            ImagePath = sn.ImagePath,
            CreatedAt = sn.CreatedAt
        }).ToList();

        return ServiceResult<List<StatusNoteDto>>.Ok(dtos);
    }
}
