using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.Concrete;

public class GoalStatusManager : IGoalStatusService
{
    private readonly IGoalStatusRepository _goalStatusRepository;
    private readonly IGoalRepository _goalRepository;

    public GoalStatusManager(IGoalStatusRepository goalStatusRepository, IGoalRepository goalRepository)
    {
        _goalStatusRepository = goalStatusRepository;
        _goalRepository = goalRepository;
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
        var datesByGoal = BuildDistinctDatesByGoal(allRecords);

        var sevenDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

        var dtos = allRecords
            .Where(gs => gs.ActivityDate >= sevenDaysAgo)
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
                StreakCount = ComputeStreakAt(datesByGoal[gs.GoalId], gs.ActivityDate)
            })
            .ToList();

        return ServiceResult<List<GoalStatusDto>>.Ok(dtos);
    }

    public async Task<ServiceResult<List<LongestStreakDto>>> GetLongestStreaksAsync(int userId)
    {
        var allRecords = await _goalStatusRepository.GetAllByUserIdAsync(userId);

        var result = allRecords
            .GroupBy(gs => gs.GoalId)
            .Select(group => new LongestStreakDto
            {
                GoalId = group.Key,
                GoalTitle = group.First().Goal.Title,
                LongestStreak = ComputeLongestStreak(DistinctSortedDates(group))
            })
            .OrderByDescending(x => x.LongestStreak)
            .ToList();

        return ServiceResult<List<LongestStreakDto>>.Ok(result);
    }

    private static Dictionary<int, List<DateOnly>> BuildDistinctDatesByGoal(List<GoalStatus> allRecords)
    {
        return allRecords
            .GroupBy(gs => gs.GoalId)
            .ToDictionary(g => g.Key, g => DistinctSortedDates(g));
    }

    private static List<DateOnly> DistinctSortedDates(IEnumerable<GoalStatus> records) =>
        records.Select(gs => gs.ActivityDate).Distinct().OrderBy(d => d).ToList();

    // Verilen tarihten geriye doğru, boşluksuz kaç gün kayıt var (o tarih dahil).
    private static int ComputeStreakAt(List<DateOnly> distinctSortedDates, DateOnly target)
    {
        var dateSet = new HashSet<DateOnly>(distinctSortedDates);
        var current = target;
        var streak = 0;

        while (dateSet.Contains(current))
        {
            streak++;
            current = current.AddDays(-1);
        }

        return streak;
    }

    // Sıralı, tekrarsız tarih listesindeki en uzun ardışık (gün bazlı) diziyi bulur.
    private static int ComputeLongestStreak(List<DateOnly> distinctSortedDates)
    {
        if (distinctSortedDates.Count == 0)
            return 0;

        var longest = 1;
        var current = 1;

        for (var i = 1; i < distinctSortedDates.Count; i++)
        {
            if (distinctSortedDates[i] == distinctSortedDates[i - 1].AddDays(1))
            {
                current++;
            }
            else
            {
                current = 1;
            }

            longest = Math.Max(longest, current);
        }

        return longest;
    }
}
