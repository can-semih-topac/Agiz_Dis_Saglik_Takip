using AgizDisSaglikTakip.Business.Abstract;
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
    private readonly IStatusNoteRepository _statusNoteRepository;

    public GoalManager(IGoalRepository goalRepository, IGoalStatusRepository goalStatusRepository, IStatusNoteRepository statusNoteRepository)
    {
        _goalRepository = goalRepository;
        _goalStatusRepository = goalStatusRepository;
        _statusNoteRepository = statusNoteRepository;
    }

    public async Task<ServiceResult<List<GoalDto>>> GetGoalsAsync(int userId)
    {
        var goals = await _goalRepository.GetByUserIdAsync(userId);

        var goalDtos = goals.Select(g => new GoalDto
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            PeriodUnit = g.PeriodUnit,
            PeriodFrequency = g.PeriodFrequency,
            Importance = g.Importance,
            TrackingType = g.TrackingType,
            CreatedAt = g.CreatedAt
        }).ToList();

        return ServiceResult<List<GoalDto>>.Ok(goalDtos);
    }

    public async Task<ServiceResult> CreateGoalAsync(int userId, CreateGoalDto dto) // yeni hedef oluşturma
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ServiceResult.Fail("Başlık boş olamaz.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            return ServiceResult.Fail("Açıklama boş olamaz.");

        if (dto.PeriodFrequency <= 0)
            return ServiceResult.Fail("Sıklık 0'dan büyük olmalı.");

        if (!Enum.IsDefined(typeof(PeriodUnit), dto.PeriodUnit))
            return ServiceResult.Fail("Geçersiz periyot birimi.");

        if (!Enum.IsDefined(typeof(Importance), dto.Importance))
            return ServiceResult.Fail("Geçersiz önem derecesi.");

        if (!Enum.IsDefined(typeof(TrackingType), dto.TrackingType))
            return ServiceResult.Fail("Geçersiz takip türü.");

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

    public async Task<ServiceResult<bool>> DeleteGoalAsync(int userId, int goalId, bool confirmed) // hedef silme (durum kayıtları varsa onay istenir)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == goalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult<bool>.Fail("Hedef bulunamadı.");

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
            {
                note.GoalStatusId = null;
                await _statusNoteRepository.UpdateAsync(note);
            }
        }

        await _goalRepository.DeleteAsync(goal);

        return ServiceResult<bool>.Ok(false, "Hedef silindi.");
    }
}
