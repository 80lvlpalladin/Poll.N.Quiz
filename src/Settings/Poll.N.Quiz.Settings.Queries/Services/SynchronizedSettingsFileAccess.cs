/*
using System.Text.Json;

namespace Poll.N.Quiz.Settings.Queries.Services;

public abstract class SynchronizedSettingsFileAccess : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    protected const string SettingsFilesDirectory = "SettingsFiles";

    protected async Task<string[]> EnumerateSettingsFilesAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            return Directory.EnumerateFiles(SettingsFilesDirectory, "*.json").ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    protected async Task<JsonDocument> ReadSettingsFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var jsonString = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonDocument.Parse(jsonString);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
    }
}
*/
