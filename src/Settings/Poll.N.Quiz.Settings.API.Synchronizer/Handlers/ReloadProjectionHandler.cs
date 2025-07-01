using ErrorOr;
using MediatR;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventStore.ReadOnly;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;

namespace Poll.N.Quiz.Settings.API.Synchronizer.Handlers;

public record ReloadProjectionRequest(string ServiceName, string EnvironmentName)
    : IRequest<ErrorOr<Success>>;

public class ReloadProjectionHandler(
    IReadOnlySettingsEventStore readOnlySettingsEventStore,
    IWriteOnlySettingsProjectionStore writeOnlySettingsProjectionStore)
    : IRequestHandler<ReloadProjectionRequest, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ReloadProjectionRequest request, CancellationToken cancellationToken)
    {
        var settingsMetadata = new SettingsMetadata(request.ServiceName, request.EnvironmentName);

        var allEvents =
            await readOnlySettingsEventStore.GetAsync(settingsMetadata, cancellationToken);

        if (allEvents.Length is 0)
        {
            return Error.NotFound("No settings found for the given service and environment");
        }

        var settingsAggregate = new SettingsAggregate(settingsMetadata);

        foreach (var @event in allEvents)
        {
            if(!settingsAggregate.TryApplyEvent(@event, out var applyEventError))
                return applyEventError.Value;
        }


        await writeOnlySettingsProjectionStore.SaveProjectionAsync(
            settingsAggregate.CurrentProjection!,
            settingsAggregate.Metadata,
            cancellationToken);

        return Result.Success;
    }
}
