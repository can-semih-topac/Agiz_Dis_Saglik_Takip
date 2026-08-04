using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.GoalStatus;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;

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

    public async Task<ServiceResult> CreateGoalStatusAsync(int userId, CreateGoalStatusDto dto)
    {
        var goal = await _goalRepository.GetAsync(g => g.Id == dto.GoalId && g.UserId == userId);
        if (goal == null)
            return ServiceResult.Fail("Hedef bulunamadı.");

        if (dto.ActivityDate > DateOnly.FromDateTime(DateTime.Today))
            return ServiceResult.Fail("Tarih gelecekte olamaz.");

        if (dto.DurationMinutes < 0)
            return ServiceResult.Fail("Süre negatif olamaz.");

        var goalStatus = new GoalStatus
        {
            GoalId = dto.GoalId,
            ActivityDate = dto.ActivityDate,
            ActivityTime = dto.ActivityTime,
            DurationMinutes = dto.DurationMinutes,
            IsApplied = dto.IsApplied,
            CreatedAt = DateTime.Now
        };

        await _goalStatusRepository.AddAsync(goalStatus);

        return ServiceResult.Ok("Durum kaydı eklendi.");
    }

    public async Task<ServiceResult<List<GoalStatusDto>>> GetLast7DaysAsync(int userId)
    {
        var records = await _goalStatusRepository.GetLast7DaysByUserIdAsync(userId);

        var dtos = records.Select(gs => new GoalStatusDto
        {
            Id = gs.Id,
            GoalId = gs.GoalId,
            GoalTitle = gs.Goal.Title,
            ActivityDate = gs.ActivityDate,
            ActivityTime = gs.ActivityTime,
            DurationMinutes = gs.DurationMinutes,
            IsApplied = gs.IsApplied
        }).ToList();

        return ServiceResult<List<GoalStatusDto>>.Ok(dtos);
    }
}
