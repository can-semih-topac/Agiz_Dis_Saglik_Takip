using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.AdminActionLog;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.Business.Concrete;

public class AdminActionLogManager : IAdminActionLogService
{
    private const int MaxRecords = 200;

    private readonly IAdminActionLogRepository _repository;

    public AdminActionLogManager(IAdminActionLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<List<AdminActionLogDto>>> GetRecentAsync()
    {
        var logs = await _repository.GetRecentAsync(MaxRecords);

        var dtos = logs.Select(l => new AdminActionLogDto
        {
            Id = l.Id,
            AdminEmail = l.AdminEmail,
            Action = l.Action,
            TargetEmail = l.TargetEmail,
            CreatedAt = l.CreatedAt
        }).ToList();

        return ServiceResult<List<AdminActionLogDto>>.Ok(dtos);
    }

    public async Task RecordAsync(string adminEmail, string action, string targetEmail)
    {
        await _repository.AddAsync(new AdminActionLog
        {
            AdminEmail = adminEmail,
            Action = action,
            TargetEmail = targetEmail,
            CreatedAt = DateTime.Now
        });
    }
}
