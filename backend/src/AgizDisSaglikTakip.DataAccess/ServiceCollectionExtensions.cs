using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;
using AgizDisSaglikTakip.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgizDisSaglikTakip.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IGoalRepository, EfGoalRepository>();
        services.AddScoped<IGoalStatusRepository, EfGoalStatusRepository>();
        services.AddScoped<IStatusNoteRepository, EfStatusNoteRepository>();
        services.AddScoped<ISuggestionRepository, EfSuggestionRepository>();

        return services;
    }
}
