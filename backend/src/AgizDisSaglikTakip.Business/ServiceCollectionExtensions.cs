using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace AgizDisSaglikTakip.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<ISuggestionService, SuggestionManager>();
        services.AddScoped<IGoalService, GoalManager>();
        services.AddScoped<IGoalStatusService, GoalStatusManager>();
        services.AddScoped<IStatusNoteService, StatusNoteManager>();
        services.AddScoped<IContactService, ContactManager>();
        services.AddScoped<IWillpowerService, WillpowerManager>();
        services.AddScoped<ILogService, LogManager>();
        services.AddScoped<IAdminActionLogService, AdminActionLogManager>();
        services.AddScoped<IDemoService, DemoManager>();

        return services;
    }
}
