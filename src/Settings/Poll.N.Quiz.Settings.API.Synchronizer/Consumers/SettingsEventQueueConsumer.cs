using ErrorOr;
using MassTransit;
using Poll.N.Quiz.Settings.Domain;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly;

namespace Poll.N.Quiz.Settings.API.Synchronizer.Consumers;

public class SettingsEventQueueConsumer(
    IWriteOnlySettingsProjectionStore writeOnlyProjectionStore,
    IReadOnlySettingsProjectionStore readOnlyProjectionStore)
    : IConsumer<SettingsEvent>
{
    public async Task Consume(ConsumeContext<SettingsEvent> context)
    {
        var settingsEvent = context.Message;

        var existingSettingsProjection =
            await readOnlyProjectionStore.GetAsync(settingsEvent.Metadata);

        var settingsAggregate = new SettingsAggregate(settingsEvent.Metadata, existingSettingsProjection);

        if (!settingsAggregate.TryApplyEvent(settingsEvent, out var applyEventError))
        {
            throw new InvalidOperationException(
                "Fail to consume SettingsEvent: " + applyEventError.Value.Description);
        }

        await writeOnlyProjectionStore.SaveProjectionAsync
            (settingsAggregate.CurrentProjection, settingsAggregate.Metadata, context.CancellationToken);
    }

}
