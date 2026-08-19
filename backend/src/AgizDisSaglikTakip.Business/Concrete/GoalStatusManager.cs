using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.Business.Utilities;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.Concrete;

public class GoalStatusManager : IGoalStatusService
{
    private readonly IGoalStatusRepository _goalStatusRepository;
    private readonly IGoalRepository _goalRepository;
    private readonly IGoalPauseRepository _goalPauseRepository;
    private readonly IStatusNoteRepository _statusNoteRepository;

    public GoalStatusManager(
        IGoalStatusRepository goalStatusRepository,
        IGoalRepository goalRepository,
        IGoalPauseRepository goalPauseRepository,
        IStatusNoteRepository statusNoteRepository)
    {
        _goalStatusRepository = goalStatusRepository;
        _goalRepository = goalRepository;
        _goalPauseRepository = goalPauseRepository;
        _statusNoteRepository = statusNoteRepository;
    }

    public async Task<ServiceResult<int>> CreateGoalStatusAsync(int userId, CreateGoalStatusDto dto)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == dto.GoalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult<int>.Fail("Hedef bulunamadı.");

        if (dto.ActivityDate > DateOnly.FromDateTime(DateTime.Today))
            return ServiceResult<int>.Fail("Tarih gelecekte olamaz.");

        // Süre, hedefin takip türüne göre zorunlu ya da anlamsız — hedef tarafı belirleyici,
        // istemci ne gönderirse göndersin burada tutarlı hale getiriyoruz.
        int? durationMinutes;
        if (goal.TrackingType == TrackingType.Sureli)
        {
            if (dto.DurationMinutes == null || dto.DurationMinutes < 0)
                return ServiceResult<int>.Fail("Bu hedef süreli takip edildiği için geçerli bir süre girilmeli.");

            durationMinutes = dto.DurationMinutes;
        }
        else
        {
            durationMinutes = null;
        }

        var goalStatus = new GoalStatus
        {
            GoalId = dto.GoalId,
            ActivityDate = dto.ActivityDate,
            ActivityTime = dto.ActivityTime,
            DurationMinutes = durationMinutes,
            CreatedAt = DateTime.Now
        };

        await _goalStatusRepository.AddAsync(goalStatus);

        return ServiceResult<int>.Ok(goalStatus.Id, "Durum kaydı eklendi.");
    }

    public async Task<ServiceResult<List<GoalStatusDto>>> GetLast7DaysAsync(int userId)
    {
        var allRecords = await _goalStatusRepository.GetAllByUserIdAsync(userId);
        var pausedDatesByGoal = await BuildPausedDatesByGoalAsync(userId);
        var sevenDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

        var dtos = MapToDtosWithStreak(allRecords, allRecords.Where(gs => gs.ActivityDate >= sevenDaysAgo), pausedDatesByGoal);

        return ServiceResult<List<GoalStatusDto>>.Ok(dtos);
    }

    // Takvim görünümü — herhangi bir ayı gezebilmek için tüm geçmiş kayıtlar gerekiyor.
    public async Task<ServiceResult<List<GoalStatusDto>>> GetAllAsync(int userId)
    {
        var allRecords = await _goalStatusRepository.GetAllByUserIdAsync(userId);
        var pausedDatesByGoal = await BuildPausedDatesByGoalAsync(userId);
        var dtos = MapToDtosWithStreak(allRecords, allRecords, pausedDatesByGoal);

        return ServiceResult<List<GoalStatusDto>>.Ok(dtos);
    }

    // Seri hesaplaması her zaman TÜM geçmişe bakmalı (aksi halde bir aylık pencerenin başındaki
    // kayıtların serisi yanlış görünür); dönen liste ise sadece "toShow" içindekilerle sınırlanır.
    private static List<GoalStatusDto> MapToDtosWithStreak(
        List<GoalStatus> allRecords, IEnumerable<GoalStatus> toShow, Dictionary<int, HashSet<DateOnly>> pausedDatesByGoal)
    {
        var datesByGoal = allRecords
            .GroupBy(gs => gs.GoalId)
            .ToDictionary(g => g.Key, g => new HashSet<DateOnly>(g.Select(gs => gs.ActivityDate)));

        return toShow
            .OrderByDescending(gs => gs.ActivityDate)
            .ThenByDescending(gs => gs.ActivityTime)
            .Select(gs => new GoalStatusDto
            {
                Id = gs.Id,
                GoalId = gs.GoalId,
                GoalTitle = gs.Goal.Title,
                TrackingType = gs.Goal.TrackingType,
                ActivityDate = gs.ActivityDate,
                ActivityTime = gs.ActivityTime,
                DurationMinutes = gs.DurationMinutes,
                StreakCount = StreakCalculator.ComputeStreakAt(
                    datesByGoal[gs.GoalId], gs.ActivityDate, pausedDatesByGoal.GetValueOrDefault(gs.GoalId))
            })
            .ToList();
    }

    public async Task<ServiceResult<List<LongestStreakDto>>> GetLongestStreaksAsync(int userId)
    {
        var allRecords = await _goalStatusRepository.GetAllByUserIdAsync(userId);
        var pausedDatesByGoal = await BuildPausedDatesByGoalAsync(userId);

        var result = allRecords
            .GroupBy(gs => gs.GoalId)
            .Select(group => new LongestStreakDto
            {
                GoalId = group.Key,
                GoalTitle = group.First().Goal.Title,
                LongestStreak = StreakCalculator.ComputeLongestStreak(
                    DistinctSortedDates(group), pausedDatesByGoal.GetValueOrDefault(group.Key))
            })
            .OrderByDescending(x => x.LongestStreak)
            .ToList();

        return ServiceResult<List<LongestStreakDto>>.Ok(result);
    }

    private async Task<Dictionary<int, HashSet<DateOnly>>> BuildPausedDatesByGoalAsync(int userId)
    {
        var pauses = await _goalPauseRepository.GetAllByUserIdAsync(userId);
        return StreakCalculator.BuildPausedDatesByGoal(pauses, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<ServiceResult> UpdateGoalStatusAsync(int userId, int id, UpdateGoalStatusDto dto)
    {
        var goalStatus = await _goalStatusRepository.GetAsync(gs => gs.Id == id && gs.Goal.UserId == userId);
        if (goalStatus == null)
            return ServiceResult.Fail("Durum kaydı bulunamadı.");

        if (dto.ActivityDate > DateOnly.FromDateTime(DateTime.Today))
            return ServiceResult.Fail("Tarih gelecekte olamaz.");

        var goal = await _goalRepository.GetAsync(g => g.Id == goalStatus.GoalId);
        if (goal == null)
            return ServiceResult.Fail("Hedef bulunamadı.");

        int? durationMinutes;
        if (goal.TrackingType == TrackingType.Sureli)
        {
            if (dto.DurationMinutes == null || dto.DurationMinutes < 0)
                return ServiceResult.Fail("Bu hedef süreli takip edildiği için geçerli bir süre girilmeli.");

            durationMinutes = dto.DurationMinutes;
        }
        else
        {
            durationMinutes = null;
        }

        goalStatus.ActivityDate = dto.ActivityDate;
        goalStatus.ActivityTime = dto.ActivityTime;
        goalStatus.DurationMinutes = durationMinutes;
        await _goalStatusRepository.UpdateAsync(goalStatus);

        return ServiceResult.Ok("Durum kaydı güncellendi.");
    }

    public async Task<ServiceResult<bool>> DeleteGoalStatusAsync(int userId, int id)
    {
        var goalStatus = await _goalStatusRepository.GetAsync(gs => gs.Id == id && gs.Goal.UserId == userId);
        if (goalStatus == null)
            return ServiceResult<bool>.Fail("Durum kaydı bulunamadı.");

        // StatusNote->GoalStatus ilişkisi NO ACTION olduğu için kaydı silmeden önce
        // bağlı notun bağlantısını koparıyoruz (GoalManager.DeleteGoalAsync'teki gibi), not silinmez.
        var linkedNotes = await _statusNoteRepository.GetByGoalStatusIdsAsync(new[] { id });
        foreach (var note in linkedNotes)
            note.GoalStatusId = null;

        if (linkedNotes.Count > 0)
            await _statusNoteRepository.UpdateRangeAsync(linkedNotes);

        // Yumuşak silme — hiçbir kayıt veritabanından fiziksel olarak gitmiyor.
        goalStatus.IsDeleted = true;
        await _goalStatusRepository.UpdateAsync(goalStatus);

        return ServiceResult<bool>.Ok(true, "Durum kaydı silindi.");
    }

    private static List<DateOnly> DistinctSortedDates(IEnumerable<GoalStatus> records) =>
        records.Select(gs => gs.ActivityDate).Distinct().OrderBy(d => d).ToList();
}
