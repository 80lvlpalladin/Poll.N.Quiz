using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;

namespace Poll.N.Quiz.Settings.API.Commands;

public static class ServiceRegistrant
{
    public static IServiceCollection AddCommandServices(
        this IServiceCollection services,
        string settingsEventStoreConnectionString) =>
        services
            .AddWriteOnlySettingsEventStore(settingsEventStoreConnectionString)
            .AddCommandHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        var commandsAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(commandsAssembly));
        return services;
    }
}
