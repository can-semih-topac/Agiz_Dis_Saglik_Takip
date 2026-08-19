using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.DataAccess.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgizDisSaglikTakip.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ILoggerProvider olarak DI'a kayıtlı — ASP.NET Core bunu otomatik olarak logging'e dahil ediyor,
        // ayrıca builder.Logging.AddProvider(...) çağırmamıza gerek yok.
        services.AddSingleton<ILoggerProvider, DbLoggerProvider>();

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IGoalRepository, EfGoalRepository>();
        services.AddScoped<IGoalStatusRepository, EfGoalStatusRepository>();
        services.AddScoped<IGoalPauseRepository, EfGoalPauseRepository>();
        services.AddScoped<IStatusNoteRepository, EfStatusNoteRepository>();
        services.AddScoped<ISuggestionRepository, EfSuggestionRepository>();
        services.AddScoped<IContactMessageRepository, EfContactMessageRepository>();
        services.AddScoped<ILogRepository, EfLogRepository>();
        services.AddScoped<IAdminActionLogRepository, EfAdminActionLogRepository>();

        return services;
    }
}
