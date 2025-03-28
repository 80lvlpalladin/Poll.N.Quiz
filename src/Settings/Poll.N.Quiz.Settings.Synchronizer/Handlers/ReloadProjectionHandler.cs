using ErrorOr;
using MediatR;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.Projection.WriteOnly;

namespace Poll.N.Quiz.Settings.Synchronizer.Handlers;

public record ReloadProjectionRequest(string ServiceName, string EnvironmentName)
    : IRequest<ErrorOr<Success>>;

//TODO cover with tests
public class ReloadProjectionHandler(
    IReadOnlySettingsEventStore readOnlySettingsEventStore,
    IWriteOnlySettingsProjection writeOnlySettingsProjection)
    : IRequestHandler<ReloadProjectionRequest, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ReloadProjectionRequest request, CancellationToken cancellationToken)
    {
        var settingsMetadata = new SettingsMetadata(request.ServiceName, request.EnvironmentName);

        var allEvents =
            await readOnlySettingsEventStore.GetAsync(settingsMetadata, cancellationToken);

        var settingsCreateEvent =
            allEvents.Single(e => e.EventType == SettingsEventType.CreateEvent);

        var settingsAggregate = new SettingsAggregate(settingsCreateEvent);

        foreach (var @event in allEvents.Skip(1))
        {
            var applyResult = settingsAggregate.ApplyEvent(@event);

            if (applyResult.IsError)
                return applyResult;
        }

        await writeOnlySettingsProjection.SaveProjectionAsync(
            settingsAggregate.CurrentProjection,
            settingsAggregate.Metadata,
            cancellationToken);

        return Result.Success;
    }
}
