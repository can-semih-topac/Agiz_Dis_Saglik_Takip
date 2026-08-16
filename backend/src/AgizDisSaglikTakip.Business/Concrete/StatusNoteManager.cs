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
    private readonly IGoalStatusRepository _goalStatusRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IFileStorageService _fileStorageService;

    public StatusNoteManager(
        IStatusNoteRepository statusNoteRepository,
        IGoalStatusRepository goalStatusRepository,
        IGoalRepository goalRepository,
        IFileStorageService fileStorageService)
    {
        _statusNoteRepository = statusNoteRepository;
        _goalStatusRepository = goalStatusRepository;
        _goalRepository = goalRepository;
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
            GoalStatusId = await ResolveOwnedGoalStatusIdAsync(userId, dto.GoalStatusId),
            CreatedAt = DateTime.Now
        };

        await _statusNoteRepository.AddAsync(statusNote);

        return ServiceResult.Ok("Not kaydedildi.");
    }

    public async Task<ServiceResult<List<StatusNoteDto>>> GetLast7DaysAsync(int userId)
    {
        var records = await _statusNoteRepository.GetLast7DaysByUserIdAsync(userId);
        return ServiceResult<List<StatusNoteDto>>.Ok(records.Select(MapToDto).ToList());
    }

    public async Task<ServiceResult<List<StatusNoteDto>>> GetAllAsync(int userId)
    {
        var records = await _statusNoteRepository.GetAllByUserIdAsync(userId);
        return ServiceResult<List<StatusNoteDto>>.Ok(records.Select(MapToDto).ToList());
    }

    private static StatusNoteDto MapToDto(StatusNote sn) => new()
    {
        Id = sn.Id,
        Description = sn.Description,
        ImagePath = sn.ImagePath,
        GoalStatusId = sn.GoalStatusId,
        CreatedAt = sn.CreatedAt
    };

    // Notu bir durum kaydına bağlamadan önce, o kaydın gerçekten bu kullanıcıya ait olduğunu doğrular.
    private async Task<int?> ResolveOwnedGoalStatusIdAsync(int userId, int? goalStatusId)
    {
        if (goalStatusId == null)
            return null;

        var goalStatus = await _goalStatusRepository.GetAsync(gs => gs.Id == goalStatusId.Value);
        if (goalStatus == null)
            return null;

        var goal = await _goalRepository.GetAsync(g => g.Id == goalStatus.GoalId && g.UserId == userId);
        return goal != null ? goalStatus.Id : null;
    }
}
