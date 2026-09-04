using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.Constants;
using AgizDisSaglikTakip.Business.DTOs.Goal;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.Concrete;

public class GoalManager : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IGoalStatusRepository _goalStatusRepository;
    private readonly IGoalPauseRepository _goalPauseRepository;
    private readonly IStatusNoteRepository _statusNoteRepository;

    public GoalManager(
        IGoalRepository goalRepository,
        IGoalStatusRepository goalStatusRepository,
        IGoalPauseRepository goalPauseRepository,
        IStatusNoteRepository statusNoteRepository)
    {
        _goalRepository = goalRepository;
        _goalStatusRepository = goalStatusRepository;
        _goalPauseRepository = goalPauseRepository;
        _statusNoteRepository = statusNoteRepository;
    }

    public async Task<ServiceResult<List<GoalDto>>> GetGoalsAsync(int userId)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId);
        var activePausesByGoalId = (await _goalPauseRepository.GetAllByUserIdAsync(userId))
            .Where(p => p.EndDate == null)
            .ToDictionary(p => p.GoalId);

        var goalDtos = goals.Select(g =>
        {
            activePausesByGoalId.TryGetValue(g.Id, out var activePause);
            return new GoalDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                PeriodUnit = g.PeriodUnit,
                PeriodFrequency = g.PeriodFrequency,
                Importance = g.Importance,
                TrackingType = g.TrackingType,
                CreatedAt = g.CreatedAt,
                IsPaused = activePause != null,
                PauseReason = activePause?.Reason,
                PausedSince = activePause?.StartDate
            };
        }).ToList();

        return ServiceResult<List<GoalDto>>.Ok(goalDtos);
    }

    public async Task<ServiceResult> PauseGoalAsync(int userId, int goalId, StartGoalPauseDto dto)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult.Fail(ErrorMessages.GoalNotFound);

        if (string.IsNullOrWhiteSpace(dto.Reason))
            return ServiceResult.Fail("Duraklatma sebebi yazılmalı.");

        var existingActive = await _goalPauseRepository.GetActivePauseAsync(goalId);
        if (existingActive != null)
            return ServiceResult.Fail("Bu hedef zaten duraklatılmış.");

        var pause = new GoalPause
        {
            GoalId = goalId,
            Reason = dto.Reason,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = null,
            CreatedAt = DateTime.Now
        };

        await _goalPauseRepository.AddAsync(pause);

        return ServiceResult.Ok("Hedef duraklatıldı.");
    }

    public async Task<ServiceResult> ResumeGoalAsync(int userId, int goalId)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult.Fail(ErrorMessages.GoalNotFound);

        var activePause = await _goalPauseRepository.GetActivePauseAsync(goalId);
        if (activePause == null)
            return ServiceResult.Fail("Bu hedef zaten duraklatılmış değil.");

        activePause.EndDate = DateOnly.FromDateTime(DateTime.Today);
        await _goalPauseRepository.UpdateAsync(activePause);

        return ServiceResult.Ok("Hedef tekrar aktif edildi.");
    }

    public async Task<ServiceResult> CreateGoalAsync(int userId, CreateGoalDto dto) // yeni hedef oluşturma
    {
        var validationError = ValidateGoalFields(dto.Title, dto.Description, dto.PeriodFrequency, dto.PeriodUnit, dto.Importance, dto.TrackingType);
        if (validationError != null)
            return ServiceResult.Fail(validationError);

        var goal = new Goal
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            PeriodUnit = dto.PeriodUnit,
            PeriodFrequency = dto.PeriodFrequency,
            Importance = dto.Importance,
            TrackingType = dto.TrackingType,
            CreatedAt = DateTime.Now
        };

        await _goalRepository.AddAsync(goal);

        return ServiceResult.Ok("Hedef oluşturuldu.");
    }

    // Hedefin kendisi (başlık/açıklama/periyot/önem/takip türü) sonradan değiştirilebiliyor —
    // buna bağlı GoalStatus kayıtlarına dokunulmuyor, ör. TrackingType değişse bile geçmiş
    // kayıtlardaki DurationMinutes olduğu gibi kalıyor (zaten nullable, sorun çıkarmıyor).
    public async Task<ServiceResult> UpdateGoalAsync(int userId, int goalId, UpdateGoalDto dto)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult.Fail(ErrorMessages.GoalNotFound);

        var validationError = ValidateGoalFields(dto.Title, dto.Description, dto.PeriodFrequency, dto.PeriodUnit, dto.Importance, dto.TrackingType);
        if (validationError != null)
            return ServiceResult.Fail(validationError);

        goal.Title = dto.Title;
        goal.Description = dto.Description;
        goal.PeriodUnit = dto.PeriodUnit;
        goal.PeriodFrequency = dto.PeriodFrequency;
        goal.Importance = dto.Importance;
        goal.TrackingType = dto.TrackingType;

        await _goalRepository.UpdateAsync(goal);

        return ServiceResult.Ok("Hedef güncellendi.");
    }

    private static string? ValidateGoalFields(
        string title, string description, int periodFrequency, PeriodUnit periodUnit, Importance importance, TrackingType trackingType)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Başlık boş olamaz.";

        if (string.IsNullOrWhiteSpace(description))
            return "Açıklama boş olamaz.";

        if (periodFrequency <= 0)
            return "Sıklık 0'dan büyük olmalı.";

        if (!Enum.IsDefined(typeof(PeriodUnit), periodUnit))
            return "Geçersiz periyot birimi.";

        if (!Enum.IsDefined(typeof(Importance), importance))
            return "Geçersiz önem derecesi.";

        if (!Enum.IsDefined(typeof(TrackingType), trackingType))
            return "Geçersiz takip türü.";

        return null;
    }

    public async Task<ServiceResult<bool>> DeleteGoalAsync(int userId, int goalId, bool confirmed) // hedef silme (durum kayıtları varsa onay istenir)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult<bool>.Fail(ErrorMessages.GoalNotFound);

        var statusRecords = await _goalStatusRepository.GetByGoalIdAsync(goalId);
        if (statusRecords.Count > 0 && !confirmed)
        {
            return ServiceResult<bool>.Ok(true, "Bu hedefe ait durum kayıtları var. Silmek istediğinize emin misiniz?");
        }

        // StatusNote->GoalStatus ilişkisi NO ACTION (SQL Server çoklu cascade yoluna izin vermiyor) —
        // hedefi silmeden önce bağlı notların bağlantısını elle koparıyoruz, notların kendisi silinmez.
        if (statusRecords.Count > 0)
        {
            var linkedNotes = await _statusNoteRepository.GetByGoalStatusIdsAsync(statusRecords.Select(gs => gs.Id));
            foreach (var note in linkedNotes)
                note.GoalStatusId = null;

            if (linkedNotes.Count > 0)
                await _statusNoteRepository.UpdateRangeAsync(linkedNotes);

            foreach (var gs in statusRecords)
                gs.IsDeleted = true;

            await _goalStatusRepository.UpdateRangeAsync(statusRecords);
        }

        // Yumuşak silme — hiçbir kayıt veritabanından fiziksel olarak gitmiyor.
        goal.IsDeleted = true;
        await _goalRepository.UpdateAsync(goal);

        return ServiceResult<bool>.Ok(false, "Hedef silindi.");
    }
}
