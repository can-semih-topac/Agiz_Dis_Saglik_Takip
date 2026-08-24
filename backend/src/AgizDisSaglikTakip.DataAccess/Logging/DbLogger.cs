using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.DataAccess.Logging;

public class DbLogger : ILogger
{
    private const string LogsIndex = "logs";

    private readonly string _category;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ElasticsearchClient _esClient;

    public DbLogger(string category, IServiceScopeFactory scopeFactory, ElasticsearchClient esClient)
    {
        _category = category;
        _scopeFactory = scopeFactory;
        _esClient = esClient;
    }

    // Information/Debug seviyesi ASP.NET Core'un kendi iç loglarıyla dolup taşar; sadece Warning ve üstünü tutuyoruz.
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        try
        {
            // DbLogger tek nesne (singleton) ama AppDbContext istek başına (scoped) — doğrudan enjekte
            // edemeyiz, her log yazımında kendi küçük scope'umuzu açıp oradan bir AppDbContext alıyoruz.
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new Log
            {
                Level = logLevel.ToString(),
                Category = _category,
                Message = formatter(state, exception),
                Exception = exception?.ToString(),
                CreatedAt = DateTime.Now
            };

            context.Logs.Add(log);
            context.SaveChanges();

            // SQL Server kalıcı gerçek kaynak (source of truth) olarak kalıyor; ElasticSearch
            // sadece admin panelindeki tam metin arama için ayrı bir kopya indeks. Aynı Id ile
            // yazıyoruz ki iki taraf birbirine karışmadan eşleşsin.
            //
            // Bilerek beklemiyoruz (fire-and-forget): senkron çağırınca .NET'in "sync-over-async"
            // tuzağına düşüp thread pool yeterince hızlı büyüyemediği için tek bir istek 10-30
            // saniyeye kadar yavaşlayabiliyordu. Loglama asla asıl isteği yavaşlatmamalı — ES'e
            // yazma arka planda tamamlanır, sonucunu beklemeyiz.
            _ = IndexToElasticAsync(log);
        }
        catch
        {
            // Loglama sırasında bir hata olursa bunu tekrar loglamaya çalışıp sonsuz döngüye girmeyelim.
        }
    }

    private async Task IndexToElasticAsync(Log log)
    {
        try
        {
            await _esClient.IndexAsync(log, idx => idx.Index(LogsIndex).Id(log.Id));
        }
        catch
        {
            // ElasticSearch'e yazamasak bile SQL Server'daki kalıcı kayıt zaten var — sessizce yut.
        }
    }
}
