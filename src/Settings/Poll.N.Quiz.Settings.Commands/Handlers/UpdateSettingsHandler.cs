using System.Text.Json;
using ErrorOr;
using FluentValidation;
using Json.Patch;
using MediatR;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventQueue;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;

namespace Poll.N.Quiz.Settings.Commands.Handlers;

public sealed record UpdateSettingsCommand(
    uint TimeStamp,
    uint Version,
    string ServiceName,
    string EnvironmentName,
    string SettingsPatchJson)
    : IRequest<ErrorOr<Success>>;

public class UpdateSettingsHandler(
    IWriteOnlySettingsEventStore settingsEventStore,
    SettingsEventQueueProducer queueProducer)
    : IRequestHandler<UpdateSettingsCommand, ErrorOr<Success>>
{
    private readonly UpdateSettingsCommandValidator _validator = new();

    public async Task<ErrorOr<Success>> Handle
        (UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        if(_validator.Validate(request) is { IsValid : false } validationResult)
            return Error.Validation(validationResult.ToString());

        var settingsUpdateEvent = new SettingsEvent(
            SettingsEventType.UpdateEvent,
            new SettingsMetadata(
                request.ServiceName,
                request.EnvironmentName),
            request.TimeStamp,
            request.Version,
            request.SettingsPatchJson);

        var result = await settingsEventStore.SaveAsync(settingsUpdateEvent, cancellationToken);

        if(!result)
            return Error.Failure($"Failed to save Settings{Enum.GetName(settingsUpdateEvent.EventType)}");

        await queueProducer.SendAsync(settingsUpdateEvent, cancellationToken);

        return Result.Success;
    }

    private sealed class UpdateSettingsCommandValidator
        : AbstractValidator<UpdateSettingsCommand>
    {
        public UpdateSettingsCommandValidator()
        {
            RuleFor(x => x.ServiceName).NotEmpty();
            RuleFor(x => x.Version).GreaterThan((uint) 0);
            RuleFor(x => x.EnvironmentName).NotEmpty();
            RuleFor(x => x.SettingsPatchJson).NotEmpty();
            RuleFor(x => x.SettingsPatchJson)
                .Must(x =>
                {
                    try
                    {
                        var value = JsonSerializer.Deserialize<JsonPatch>(x);
                        return value is not null;
                    }
                    catch(JsonException)
                    {
                        return false;
                    }
                });
        }
    }
}
