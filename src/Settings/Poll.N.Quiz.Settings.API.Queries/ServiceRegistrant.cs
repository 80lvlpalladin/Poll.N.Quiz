using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

namespace Poll.N.Quiz.Settings.API.Queries;

public static class ServiceRegistrant
{
    public static IServiceCollection AddQueryServices
        (this IServiceCollection services, string settingsProjectionStoreConnectionString) =>
        services
            .AddReadOnlySettingsProjectionStore(settingsProjectionStoreConnectionString)
            .AddQueryHandlers();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        var queriesAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(queriesAssembly));
        return services;
    }
}
