using System.Net.Http.Json;
using Poll.N.Quiz.Clients;
using Poll.N.Quiz.Clients.Payloads;
using Poll.N.Quiz.ServiceDiscovery;
using Poll.N.Quiz.Settings.FileStore.ReadOnly;

namespace Poll.N.Quiz.AppHost;

public class SettingsEventStoreInitializer(
    IReadOnlySettingsFileStore readOnlySettingsFileStore,
    ISettingsApiClient settingsApiClient)
{

    private const string CreateSettingsEndpoint = "api/v1/settings";
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var storedSettingsMetadata =
            readOnlySettingsFileStore.GetAllSettingsMetadata(cancellationToken);

        foreach (var metadata in storedSettingsMetadata)
        {
            var settingsFileContent =
                await readOnlySettingsFileStore.GetSettingsContentAsync(metadata, cancellationToken);

            var webRequestBody = new CreateSettingsRequest(
                Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                0,
                metadata.ServiceName,
                metadata.EnvironmentName,
                settingsFileContent);

            Console.WriteLine("Starting request to create or update settings for " +
                              $"{metadata.ServiceName} in {metadata.EnvironmentName} environment");
            try
            {
                await settingsApiClient.CreateSettingsAsync(webRequestBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
