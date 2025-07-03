using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;

namespace Poll.N.Quiz.API;

public static class Endpoints
{
    private const string BaseApiUrl = "api/v1";

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var routeGroupBuilder = routeBuilder.MapGroup(BaseApiUrl);

        routeGroupBuilder.MapGet("/info", GetApplicationInfo);

        return routeBuilder;
    }

    private static IResult GetApplicationInfo(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();
        var framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
        var process = Process.GetCurrentProcess();
        var appInfo = new Dictionary<string, object?>
        {
            { "ApplicationName", assemblyName.Name },
            { "AssemblyVersion", assemblyName.Version?.ToString() },
            { "Framework", framework },
            { "Environment", environment.EnvironmentName },
            { "Uptime", DateTime.Now - process.StartTime },
            { "ProcessId", process.Id },
            { "MemoryUsage, bytes", process.WorkingSet64 },
            { "Configuration", GetAppSettingsConfiguration(configuration) }
        };

        return Results.Ok(appInfo);
    }

    private static Dictionary<string, string> GetAppSettingsConfiguration(IConfiguration configuration)
    {
        var appSettingsConfiguration = new Dictionary<string, string>();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var keyValuePair in configuration.AsEnumerable())
        {
            if (keyValuePair.Key.Contains(':') &&
                !string.IsNullOrWhiteSpace(keyValuePair.Value) &&
                !IsSecret(keyValuePair.Key))
            {
                appSettingsConfiguration.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        return appSettingsConfiguration;
    }

    private static bool IsSecret(string key)
    {
        string[] secretIndicators =
            [ "Secret", "Key", "Password", "Token", "ConnectionString", "Private", "Id", "Certificate" ];

        return secretIndicators.Any(indicator => key.Contains(indicator, StringComparison.InvariantCultureIgnoreCase));
    }
}


