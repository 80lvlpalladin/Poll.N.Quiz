using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;

namespace Poll.N.Quiz.Settings.API.Synchronizer;

public static class ServiceRegistrant
{
    public static IServiceCollection AddSynchronizerServices(
        this IServiceCollection services,
            IConfiguration configuration,
            string settingsProjectionStoreConnectionString,
            string settingsEventStoreConnectionString) =>
        services
            .AddWriteOnlySettingsProjectionStore(configuration, settingsProjectionStoreConnectionString)
            .AddReadOnlySettingsProjectionStore(settingsProjectionStoreConnectionString)
            .AddReadOnlySettingsEventStore(settingsEventStoreConnectionString)
            .AddSynchronizerHandlers();

    private static IServiceCollection AddSynchronizerHandlers(this IServiceCollection services)
    {
        var synchronizerAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(synchronizerAssembly));
        return services;
    }
}
