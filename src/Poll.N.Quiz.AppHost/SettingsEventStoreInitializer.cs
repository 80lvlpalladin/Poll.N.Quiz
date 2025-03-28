using System.Net.Http.Json;
using Poll.N.Quiz.Settings.FileStore.ReadOnly;

namespace Poll.N.Quiz.AppHost;

public class SettingsEventStoreInitializer(
    IReadOnlySettingsFileStore readOnlySettingsFileStore)
{
    private const string CreateSettingsEndpoint = "api/v1/settings";
    public async Task ExecuteAsync(
        string settingsApiBaseAddress,
        CancellationToken cancellationToken = default)
    {
        var storedSettingsMetadata =
            readOnlySettingsFileStore.GetAllSettingsMetadata(cancellationToken);
        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(settingsApiBaseAddress);

        foreach (var metadata in storedSettingsMetadata)
        {
            var settingsFileContent =
                await readOnlySettingsFileStore.GetSettingsContentAsync(metadata, cancellationToken);

            var webRequestBody = new CreateOrUpdateSettingsWebRequest(
                Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                0,
                metadata.ServiceName,
                metadata.EnvironmentName,
                settingsFileContent);

            var response = await httpClient.PostAsJsonAsync
                (CreateSettingsEndpoint, webRequestBody, cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }

    private sealed record CreateOrUpdateSettingsWebRequest(
        uint TimeStamp,
        uint Version,
        string ServiceName,
        string EnvironmentName,
        string JsonData);
}
