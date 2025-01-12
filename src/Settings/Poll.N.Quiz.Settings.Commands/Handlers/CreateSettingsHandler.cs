using System.Text.Json;
using ErrorOr;
using FluentValidation;
using MassTransit;
using MediatR;
using Poll.N.Quiz.Settings.EventStore.WriteOnly;
using Poll.N.Quiz.Settings.Messaging.Contracts;

namespace Poll.N.Quiz.Settings.Commands.Handlers;

public sealed record CreateSettingsCommand(
    uint TimeStamp,
    string ServiceName,
    string EnvironmentName,
    string SettingsJson)
    : IRequest<ErrorOr<Success>>;

public class CreateSettingsHandler(
    IWriteOnlySettingsEventStore settingsEventStore,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateSettingsCommand, ErrorOr<Success>>
{
    private readonly CreateSettingsCommandValidator _validator = new();

    public async Task<ErrorOr<Success>> Handle
        (CreateSettingsCommand request, CancellationToken cancellationToken)
    {
        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        if(_validator.Validate(request) is { IsValid : false } validationResult)
            return Error.Validation(validationResult.ToString());

        var settingsCreateEvent = new SettingsEvent(
            SettingsEventType.CreateEvent,
            request.TimeStamp,
            request.ServiceName,
            request.EnvironmentName,
            request.SettingsJson);

        var result = await settingsEventStore.SaveAsync(settingsCreateEvent, cancellationToken);

        if(!result)
            return Error.Failure($"Failed to save Settings{Enum.GetName(settingsCreateEvent.EventType)}");

        await publishEndpoint.Publish(settingsCreateEvent, cancellationToken);

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
