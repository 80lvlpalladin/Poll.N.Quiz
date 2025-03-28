using System.Text.Json;
using ErrorOr;
using FluentValidation;
using MediatR;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.EventQueue;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;

namespace Poll.N.Quiz.Settings.Commands.Handlers;

public sealed record CreateSettingsCommand(
    uint TimeStamp,
    uint Version,
    string ServiceName,
    string EnvironmentName,
    string SettingsJson)
    : IRequest<ErrorOr<Success>>;

public class CreateSettingsHandler(
    IWriteOnlySettingsEventStore settingsEventStore,
    SettingsEventQueueProducer queueProducer)
    : IRequestHandler<CreateSettingsCommand, ErrorOr<Success>>
{
    private readonly CreateSettingsCommandValidator _validator = new();

    public async Task<ErrorOr<Success>> Handle
        (CreateSettingsCommand request, CancellationToken cancellationToken)
    {
        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        if(_validator.Validate(request) is { IsValid : false } validationResult)
            return Error.Validation(validationResult.ToString());
        const uint settingsCreateEventVersion = 0;

        var settingsCreateEvent = new SettingsEvent(
            SettingsEventType.CreateEvent,
            new SettingsMetadata(
                request.ServiceName,
                request.EnvironmentName),
            request.TimeStamp,
            settingsCreateEventVersion,
            request.SettingsJson);

        var result = await settingsEventStore.SaveAsync(settingsCreateEvent, cancellationToken);

        if(!result)
            return Error.Failure($"Failed to save Settings{Enum.GetName(settingsCreateEvent.EventType)}");

        await queueProducer.SendAsync(settingsCreateEvent, cancellationToken);

        return Result.Success;
    }

    private sealed class CreateSettingsCommandValidator
        : AbstractValidator<CreateSettingsCommand>
    {
        public CreateSettingsCommandValidator()
        {
            RuleFor(x => x.ServiceName).NotEmpty();
            RuleFor(x => x.EnvironmentName).NotEmpty();
            RuleFor(x => x.SettingsJson).NotEmpty();
            RuleFor(x => x.Version).Equal((uint) 0);
            RuleFor(x => x.SettingsJson)
                .Must(x =>
                {
                    try
                    {
                        JsonDocument.Parse(x);
                        return true;
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                });
        }
    }
}
