using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.Projection.WriteOnly;
using Poll.N.Quiz.Settings.Synchronizer.Services;

namespace Poll.N.Quiz.Settings.Synchronizer;

public static class ServiceRegistrant
{
    public static IServiceCollection AddSynchronizerServices
        (this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddWriteOnlySettingsProjection(configuration)
            .AddReadOnlySettingsUpdateEventStore(configuration)
            .AddSingleton<ProjectionUpdater>();
}
