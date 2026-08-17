using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Log;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;

namespace AgizDisSaglikTakip.Business.Concrete;

public class LogManager : ILogService
{
    private const int MaxRecords = 200;

    private readonly ILogRepository _logRepository;

    public LogManager(ILogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<ServiceResult<List<LogDto>>> GetRecentAsync()
    {
        var logs = await _logRepository.GetRecentAsync(MaxRecords);

        var dtos = logs.Select(l => new LogDto
        {
            Id = l.Id,
            Level = l.Level,
            Category = l.Category,
            Message = l.Message,
            Exception = l.Exception,
            CreatedAt = l.CreatedAt
        }).ToList();

        return ServiceResult<List<LogDto>>.Ok(dtos);
    }
}
