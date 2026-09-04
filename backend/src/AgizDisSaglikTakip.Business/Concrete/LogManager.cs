using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Log;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;
using Elastic.Clients.Elasticsearch;

namespace AgizDisSaglikTakip.Business.Concrete;

public class LogManager : ILogService
{
    private const int MaxRecords = 200;
    private const string LogsIndex = "logs";
    private static readonly string[] SearchFields = { "message", "exception", "category" };

    private readonly ILogRepository _logRepository;
    private readonly ElasticsearchClient _esClient;

    public LogManager(ILogRepository logRepository, ElasticsearchClient esClient)
    {
        _logRepository = logRepository;
        _esClient = esClient;
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

    // SQL Server yerine ElasticSearch'e sorguluyoruz — Message/Exception/Category alanlarında
    // tam metin arama yapıyor (SQL'deki "LIKE %kelime%" gibi değil, kelime köküne göre arıyor).
    public async Task<ServiceResult<List<LogDto>>> SearchAsync(string keyword)
    {
        var response = await _esClient.SearchAsync<Log>(s => s
            .Indices(LogsIndex)
            .Size(MaxRecords)
            .Query(q => q
                .MultiMatch(m => m
                    .Query(keyword)
                    .Fields(SearchFields)
                )
            )
            .Sort(so => so.Field(f => f.CreatedAt, o => o.Order(SortOrder.Desc)))
        );

        if (!response.IsValidResponse)
            return ServiceResult<List<LogDto>>.Fail("Arama yapılamadı.");

        var dtos = response.Documents.Select(l => new LogDto
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

    // SQL Server'daki TÜM logları ElasticSearch'e yeniden yazar. Normal akışta her log ikisine
    // birden aynı anda yazılıyor (bkz. DbLogger) ama ElasticSearch bir süre kapalı/erişilemez
    // olursa o sıradaki loglar sadece SQL'de kalır — bu metod aradaki farkı kapatır.
    public async Task<ServiceResult> ReindexAsync()
    {
        var logs = await _logRepository.GetAllAsync();

        if (logs.Count == 0)
            return ServiceResult.Ok("Yeniden indekslenecek log yok.");

        var response = await _esClient.BulkAsync(b => b
            .Index(LogsIndex)
            .IndexMany(logs, (descriptor, log) => descriptor.Id(log.Id))
        );

        if (!response.IsValidResponse)
            return ServiceResult.Fail("Yeniden indeksleme başarısız.");

        return ServiceResult.Ok($"{logs.Count} log ElasticSearch'e yeniden indekslendi.");
    }
}
