using MassTransit;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.Projection.ReadOnly;
using Poll.N.Quiz.Settings.Projection.WriteOnly;

namespace Poll.N.Quiz.Settings.Synchronizer.Consumers;

public class SettingsEventQueueConsumer(
    IWriteOnlySettingsProjection writeOnlyProjection,
    IReadOnlySettingsProjection readOnlyProjection)
    : IConsumer<SettingsEvent>
{
    public async Task Consume(ConsumeContext<SettingsEvent> context)
    {
        var settingsEvent = context.Message;
        var settingsMetadata = settingsEvent.Metadata;

        var existingSettingsProjection =
            await readOnlyProjection.GetAsync(settingsEvent.Metadata);

        var settingsAggregate = settingsEvent.EventType switch
        {
            SettingsEventType.CreateEvent when existingSettingsProjection is not null =>
                throw new InvalidOperationException(
                $"Error while consuming SettingsCreateEvent: Settings projection already exists for {settingsMetadata.ServiceName}, {settingsMetadata.EnvironmentName}"),
            SettingsEventType.CreateEvent => new SettingsAggregate(settingsEvent),
            SettingsEventType.UpdateEvent when existingSettingsProjection is null =>
                throw new InvalidOperationException(
                $"Error while consuming SettingsUpdateEvent: Settings projection does not exist for {settingsMetadata.ServiceName}, {settingsMetadata.EnvironmentName}"),
            SettingsEventType.UpdateEvent => new SettingsAggregate(settingsMetadata, existingSettingsProjection),
            _ => throw new ArgumentOutOfRangeException($"Unknown Settings Event Type: {settingsEvent.EventType}")
        };

        await writeOnlyProjection.SaveProjectionAsync
            (settingsAggregate.CurrentProjection, settingsAggregate.Metadata, context.CancellationToken);
    }
}
