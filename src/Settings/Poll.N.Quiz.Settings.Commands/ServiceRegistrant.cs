using System.Data;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;

namespace Poll.N.Quiz.Settings.Commands;

public static class ServiceRegistrant
{
    public static IServiceCollection AddCommandServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string settingsEventStoreConnectionString,
        string settingsEventQueueConnectionString) =>
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
