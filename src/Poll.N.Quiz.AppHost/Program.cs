
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.AppHost;
using Poll.N.Quiz.Settings.FileStore.ReadOnly;

var builder = DistributedApplication.CreateBuilder(args);

var eventStore = builder
    .AddMongoDB(Poll.N.Quiz.Aspire.ResourceNames.SettingsEventStore)
    .WithLifetime(ContainerLifetime.Session);

var projection = builder
    .AddRedis(Poll.N.Quiz.Aspire.ResourceNames.SettingsProjection)
    .WithLifetime(ContainerLifetime.Session);

var eventQueue = builder
    .AddKafka(Poll.N.Quiz.Aspire.ResourceNames.SettingsEventQueue)
    .WithLifetime(ContainerLifetime.Session)
    .WithKafkaUI()
    .WithLifetime(ContainerLifetime.Session);

var settingsApi = builder
    .AddProject<Projects.Poll_N_Quiz_Settings_API>(Poll.N.Quiz.Aspire.ResourceNames.SettingsApi)
    .WithReference(eventStore)
    .WaitFor(eventStore)
    .WithReference(projection)
    .WaitFor(projection)
    .WithReference(eventQueue)
    .WaitFor(eventQueue);

builder.Services
    .AddReadOnlySettingsFileStore(Path.Combine(Environment.CurrentDirectory, "SettingsFiles"))
    .AddSingleton<SettingsEventStoreInitializer>();

builder.Eventing.Subscribe<ResourceReadyEvent>(
    settingsApi.Resource,
    async (@event, cancellationToken) =>
    {
        Console.WriteLine("ResourceReadyEvent");

        var settingsApiBaseAddress = @event.Resource.Annotations
                    .OfType<EndpointAnnotation>()
                    .Single()
                    .AllocatedEndpoint
                    ?.UriString ??
                        throw new InvalidOperationException($"{Poll.N.Quiz.Aspire.ResourceNames.SettingsApi} Resource Endpoint not found");

        var settingsStoreIntializer =
            builder.Services.BuildServiceProvider().GetRequiredService<SettingsEventStoreInitializer>();

        await settingsStoreIntializer.ExecuteAsync(settingsApiBaseAddress, cancellationToken);
    });


builder.Build().Run();
