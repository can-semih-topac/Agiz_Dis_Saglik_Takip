using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.DataAccess.Logging;

// DI konteynerine ILoggerProvider olarak kaydediliyor (bkz. ServiceCollectionExtensions) —
// böylece ASP.NET Core bunu otomatik olarak logging sistemine dahil ediyor ve
// constructor'a IServiceScopeFactory gibi normal servisler enjekte edilebiliyor.
public class DbLoggerProvider : ILoggerProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DbLoggerProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ILogger CreateLogger(string categoryName) => new DbLogger(categoryName, _scopeFactory);

    public void Dispose()
    {
    }
}
