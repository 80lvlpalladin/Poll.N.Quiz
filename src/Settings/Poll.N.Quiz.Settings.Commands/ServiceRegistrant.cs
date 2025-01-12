using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Poll.N.Quiz.Settings.Commands;

public static class ServiceRegistrant
{
    public static IServiceCollection AddCommandServices(this IServiceCollection services) =>
        services
            .AddSingleton<MongoClient>()
            .AddCommandHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        var commandsAssembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(commandsAssembly));
        return services;
    }
}
