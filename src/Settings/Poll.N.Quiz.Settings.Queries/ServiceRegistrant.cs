using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Projection.ReadOnly;

namespace Poll.N.Quiz.Settings.Queries;

public static class ServiceRegistrant
{
    public static IServiceCollection AddQueryServices
        (this IServiceCollection services, string settingsProjectionConnectionString) =>
        services
            .AddReadOnlySettingsProjection(settingsProjectionConnectionString)
            .AddQueryHandlers();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        var queriesAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(queriesAssembly));
        return services;
    }
}
