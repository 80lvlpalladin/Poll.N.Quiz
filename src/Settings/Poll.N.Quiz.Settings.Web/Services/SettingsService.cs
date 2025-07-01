using Poll.N.Quiz.Clients;
using Poll.N.Quiz.ServiceDiscovery;
using Poll.N.Quiz.Settings.Web.Models;
using Refit;

namespace Poll.N.Quiz.Settings.Web.Services;

public class SettingsService(ISettingsApiClient apiClient)
{
    public async Task<IEnumerable<Service>> GetAllMetadataAsync()
    {
        var getAllMetadataResponse = await apiClient.GetAllMetadataAsync();

        return getAllMetadataResponse.Metadata.Select(metadata => new Service(
            Name: metadata.ServiceName,
            Environments: metadata.EnvironmentNames.Select(env => new Models.Environment(env))));
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettingsService(this IServiceCollection services)
    {
        var settingsApiBaseAddress =
            ConnectionStringResolver.GetHardcodedConnectionString(AspireResource.SettingsApi);

        services
            .AddRefitClient<ISettingsApiClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(settingsApiBaseAddress);
                //client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", settingsApiBaseAddress);
            });

        services.AddSingleton<SettingsService>();

        return services;
    }
}
