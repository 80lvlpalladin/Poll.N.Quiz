/*
namespace Poll.N.Quiz.Settings.Queries.Services;

public class SettingsMetadataProvider : SynchronizedSettingsFileAccess
{
    public async Task<IReadOnlyCollection<SettingsMetadata>> GetAllSettingsMetadataAsync
        (CancellationToken cancellationToken = default)
    {
        var filePaths = await EnumerateSettingsFilesAsync(cancellationToken);

        return filePaths
            .Select(GetRawMetadataFromFilePath)
            .GroupBy(se => se.ServiceName)
            .Select(grouping => new SettingsMetadata(
                grouping.Key,
                grouping.Select(se => se.EnvironmentName).ToArray()))
            .ToArray();
    }

    private static (string ServiceName, string EnvironmentName) GetRawMetadataFromFilePath(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);

        var nameParts = fileName.Split('_');

        if (nameParts.Length != 2)
            throw new InvalidOperationException("Invalid settings file name format");

        return (nameParts[0], nameParts[1]);
    }
}

public record SettingsMetadata(string ServiceName, IReadOnlyCollection<string> EnvironmentNames);
*/
