using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.Projection.ReadOnly;
using Poll.N.Quiz.Settings.Projection.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Consumers;

namespace Poll.N.Quiz.Settings.Synchronizer;

public static class ServiceRegistrant
{
    public static IServiceCollection AddSynchronizerServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string settingsProjectionConnectionString,
        string settingsEventStoreConnectionString) =>
        services
            .AddSettingsProjectionOptions(configuration)
            .AddWriteOnlySettingsProjection(settingsProjectionConnectionString)
            .AddReadOnlySettingsProjection(settingsProjectionConnectionString)
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
