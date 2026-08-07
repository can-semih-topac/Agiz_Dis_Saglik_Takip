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

        return services;
    }
}
