using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.DataAccess.Logging;

public class DbLogger : ILogger
{
    private readonly string _category;
    private readonly IServiceScopeFactory _scopeFactory;

    public DbLogger(string category, IServiceScopeFactory scopeFactory)
    {
        _category = category;
        _scopeFactory = scopeFactory;
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

            context.Logs.Add(new Log
            {
                Level = logLevel.ToString(),
                Category = _category,
                Message = formatter(state, exception),
                Exception = exception?.ToString(),
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();
        }
        catch
        {
            // Loglama sırasında bir hata olursa bunu tekrar loglamaya çalışıp sonsuz döngüye girmeyelim.
        }
    }
}
