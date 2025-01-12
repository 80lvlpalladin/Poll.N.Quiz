/*
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Poll.N.Quiz.Settings.Queries.Services;

public class FileSettingsRepository(IMemoryCache cache) : SynchronizedSettingsFileAccess
{
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(10);

    public async ValueTask<JsonDocument> GetSettingsAsync
        (string serviceName, string environmentName, CancellationToken cancellationToken = default)
    {
        var cacheKey = CreateCacheKey(serviceName, environmentName);

        if (cache.TryGetValue(cacheKey, out JsonDocument? document) && document != null)
            return document;

        var filePath = CreateFilePath(serviceName, environmentName);

        var jsonDocument = await ReadSettingsFromFileAsync(filePath, cancellationToken);

        cache.Set(cacheKey, jsonDocument, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        });

        return jsonDocument;
    }

    public void ClearCache()
    {
        if (cache is not MemoryCache memoryCache)
            throw new NotImplementedException("The following implementation works only if injected cache is of type MemoryCache");

        memoryCache.Clear();
    }

    private static string CreateCacheKey(string serviceName, string environmentName) =>
        $"{serviceName}_{environmentName}".ToLowerInvariant();

    private static string CreateFilePath(string serviceName, string environmentName) =>
        Path.Combine(SettingsFilesDirectory, $"{CreateCacheKey(serviceName, environmentName)}.json");
}
*/
