using MassTransit;
using Poll.N.Quiz.Settings.Messaging.Contracts;
using Poll.N.Quiz.Settings.Projection.ReadOnly;
using Poll.N.Quiz.Settings.Projection.WriteOnly;

namespace Poll.N.Quiz.Settings.Synchronizer.Consumers;

public class SettingsEventConsumer(
    IWriteOnlySettingsProjection writeOnlyProjection,
    IReadOnlySettingsProjection readOnlyProjection)
    : IConsumer<SettingsEvent>
{
    public async Task Consume(ConsumeContext<SettingsEvent> context)
    {
        var existingSettingsProjection = await readOnlyProjection.GetAsync
            (context.Message.ServiceName, context.Message.EnvironmentName);

        var settingsEvent = context.Message;

        if(settingsEvent.EventType is SettingsEventType.CreateEvent)
        {
            if (existingSettingsProjection is not null) throw new InvalidOperationException(
                $"Settings projection already exists for {settingsEvent.ServiceName}, {settingsEvent.EnvironmentName}");

            await writeOnlyProjection.SaveProjectionAsync(
                settingsEvent.TimeStamp,
                settingsEvent.ServiceName,
                settingsEvent.EnvironmentName,
                settingsEvent.JsonData,
                context.CancellationToken);

        }

        else if(settingsEvent.EventType is SettingsEventType.UpdateEvent)
        {
            if(existingSettingsProjection is null)
                throw new InvalidOperationException(
                    $"Settings projection does not exist for {settingsEvent.ServiceName}, {settingsEvent.EnvironmentName}");

            if(existingSettingsProjection.Value.lastUpdatedTimestamp > settingsEvent.TimeStamp)
                throw new InvalidOperationException("Event is older than the current projection");

            var newSettingsProjection = settingsEvent.JsonData
                .ApplyPatchTo(existingSettingsProjection.Value.settingsJson);

            await writeOnlyProjection.SaveProjectionAsync(
                settingsEvent.TimeStamp,
                settingsEvent.ServiceName,
                settingsEvent.EnvironmentName,
                newSettingsProjection,
                context.CancellationToken);
        }
    }
}
