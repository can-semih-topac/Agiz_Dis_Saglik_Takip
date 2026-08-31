using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.DataAccess.Logging;

// DI konteynerine ILoggerProvider olarak kaydediliyor (bkz. ServiceCollectionExtensions) —
// böylece ASP.NET Core bunu otomatik olarak logging sistemine dahil ediyor ve
// constructor'a IServiceScopeFactory gibi normal servisler enjekte edilebiliyor.
public class DbLoggerProvider : ILoggerProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ElasticsearchClient _esClient;

    public DbLoggerProvider(IServiceScopeFactory scopeFactory, ElasticsearchClient esClient)
    {
        _scopeFactory = scopeFactory;
        _esClient = esClient;
    }

    public ILogger CreateLogger(string categoryName) => new DbLogger(categoryName, _scopeFactory, _esClient);

    public void Dispose()
    {
    }
}
