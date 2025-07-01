using MediatR;
using ErrorOr;
using FluentValidation;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

namespace Poll.N.Quiz.Settings.API.Queries.Handlers;

public sealed record GetSettingsContentResponse(
    string JsonData,
    uint LastUpdatedTimestamp,
    uint Version);

public sealed record GetSettingsContentQuery(string ServiceName, string EnvironmentName) :
    IRequest<ErrorOr<GetSettingsContentResponse>>;

public sealed class GetSettingsContentHandler(IReadOnlySettingsProjectionStore projection) :
    IRequestHandler<GetSettingsContentQuery, ErrorOr<GetSettingsContentResponse>>
{
    private static readonly GetSettingsContentQueryValidator _validator = new();

    public async Task<ErrorOr<GetSettingsContentResponse>> Handle
        (GetSettingsContentQuery request, CancellationToken cancellationToken)
    {
        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        if(_validator.Validate(request) is { IsValid : false } validationResult)
            return Error.Validation(validationResult.ToString());

        var settingsMetadata =
            new SettingsMetadata(request.ServiceName, request.EnvironmentName);

        var settingsProjection = await projection.GetAsync(settingsMetadata);

        if (settingsProjection is null)
            return Error.NotFound("No settings found for the given service and environment");

        return new GetSettingsContentResponse(
            settingsProjection.JsonData,
            settingsProjection.LastUpdatedTimestamp,
            settingsProjection.Version);
    }

    private sealed class GetSettingsContentQueryValidator : AbstractValidator<GetSettingsContentQuery>
    {
        public GetSettingsContentQueryValidator()
        {
            RuleFor(x => x.ServiceName).NotEmpty();
            RuleFor(x => x.EnvironmentName).NotEmpty();
        }
    }
}
