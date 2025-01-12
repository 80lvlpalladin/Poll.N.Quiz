using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.Messaging.Contracts;
using Poll.N.Quiz.Settings.Projection.WriteOnly;

namespace Poll.N.Quiz.Settings.Synchronizer.Services;

public class ProjectionUpdater(
    IReadOnlySettingsEventStore settingsEventStore,
    IWriteOnlySettingsProjection writeOnlySettingsProjection)
{
    public async Task InitializeProjectionAsync(CancellationToken cancellationToken)
    {
        var allEvents =
            await settingsEventStore.GetAllEventsAsync(cancellationToken);

        foreach (var grouping in allEvents.GroupBy(se => new { se.ServiceName, se.EnvironmentName}))
        {
            if(cancellationToken.IsCancellationRequested)
                break;

            var createProjectionResult =
                CreateProjectionFromEvents(grouping);

            await writeOnlySettingsProjection.SaveProjectionAsync(
                createProjectionResult.creationDate,
                grouping.Key.ServiceName,
                grouping.Key.EnvironmentName,
                createProjectionResult.projection,
                cancellationToken);
        }
    }

    private static (string projection, uint creationDate)
        CreateProjectionFromEvents(params IEnumerable<SettingsEvent> settingsEvents)
    {
        var orderedEvents = settingsEvents.OrderBy(se => se.TimeStamp).ToArray();

        var settingsCreateEvent = orderedEvents.First();

        if(settingsCreateEvent.EventType is not  SettingsEventType.CreateEvent)
            throw new InvalidOperationException("First event must be a create event");

        var projectionCreationDate = orderedEvents.Last().TimeStamp;

        var initialSettingsJsonNode = JsonNode.Parse(settingsCreateEvent.JsonData);

        if(initialSettingsJsonNode is null)
            throw new InvalidOperationException("Failed to parse json data"); //Todo log this

        var jsonPatches = orderedEvents
            .Skip(1)
            .Select(ue => JsonSerializer.Deserialize<JsonPatch>(ue.JsonData))
            .Cast<JsonPatch>();

        var resultSettingsJsonNode = initialSettingsJsonNode.ApplyPatches(jsonPatches);

        return new (resultSettingsJsonNode.ToJsonString(), projectionCreationDate);
    }
}
