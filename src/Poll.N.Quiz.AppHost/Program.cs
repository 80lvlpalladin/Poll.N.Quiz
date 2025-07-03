using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.AppHost;
using Poll.N.Quiz.Infrastructure.Clients;
using Poll.N.Quiz.Infrastructure.ServiceDiscovery;
using Poll.N.Quiz.Settings.FileStore.ReadOnly;
using Refit;

var builder = DistributedApplication.CreateBuilder(args);

var eventStore = builder
    .AddMongoDB(nameof(AspireResource.SettingsEventStore))
    .WithLifetime(ContainerLifetime.Session);

var projection = builder
    .AddRedis(nameof(AspireResource.SettingsProjectionStore))
    .WithLifetime(ContainerLifetime.Session);

var eventQueue = builder
    .AddKafka(nameof(AspireResource.SettingsEventQueue))
    .WithLifetime(ContainerLifetime.Session)
    .WithKafkaUI()
    .WithLifetime(ContainerLifetime.Session);


//TODO fix the endpoint for this resource not showing up in Aspire Dashboard
var settingsApi = builder
    .AddProject<Projects.Poll_N_Quiz_Settings_API>(nameof(AspireResource.SettingsApi))
    .WithEnvironment(
        name: "ASPNETCORE_URLS",
        value: ConnectionStringResolver.GetHardcodedConnectionString(AspireResource.SettingsApi))
    .WithReference(eventStore)
    .WaitFor(eventStore)
    .WithReference(projection)
    .WaitFor(projection)
    .WithReference(eventQueue)
    .WaitFor(eventQueue);

var settingsWeb = builder
    .AddProject<Projects.Poll_N_Quiz_Settings_Web>(nameof(AspireResource.SettingsWeb))
    .WithReference(settingsApi)
    .WaitFor(settingsApi);

//For CORS configuration. without it, backend will not accept requests from settings web client
settingsApi
    .WithReference(settingsWeb);

var api = builder
    .AddProject<Projects.Poll_N_Quiz_API>(nameof(AspireResource.Api)).WithEnvironment(
        name: "ASPNETCORE_URLS",
        value: ConnectionStringResolver.GetHardcodedConnectionString(AspireResource.Api))
    .WithReference(settingsApi)
    .WaitFor(settingsApi);

builder.Services
    .AddReadOnlySettingsFileStore(Path.Combine(Environment.CurrentDirectory, "SettingsFiles"))
    .AddRefitClient<ISettingsApiClient>()
    .ConfigureHttpClient(client =>
        client.BaseAddress = new Uri(ConnectionStringResolver.GetHardcodedConnectionString(AspireResource.SettingsApi)))
    .AddStandardResilienceHandler();

builder.Services.AddSingleton<SettingsEventStoreInitializer>();

builder.Eventing.Subscribe<ResourceReadyEvent>(
    settingsApi.Resource,
    static (@event, cancellationToken) =>
    {
        var settingsEventStoreInitializer =
            @event.Services.GetRequiredService<SettingsEventStoreInitializer>();

        return settingsEventStoreInitializer.ExecuteAsync(cancellationToken);
    });

builder.Build().Run();

