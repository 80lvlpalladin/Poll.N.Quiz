using ErrorOr;
using MediatR;
using Poll.N.Quiz.Settings.Projection.ReadOnly;
using Poll.N.Quiz.Settings.Projection.ReadOnly.Entities;

namespace Poll.N.Quiz.Settings.Queries.Handlers;

public record GetAllSettingsMetadataQuery() : IRequest<ErrorOr<GetAllSettingsMetadataResponse>>;
public record GetAllSettingsMetadataResponse(IEnumerable<SettingsMetadataResponse> Metadata);
public record SettingsMetadataResponse(string ServiceName, IEnumerable<string> EnvironmentNames);

public class GetAllSettingsMetadataHandler(IReadOnlySettingsProjection projection) :
    IRequestHandler<GetAllSettingsMetadataQuery, ErrorOr<GetAllSettingsMetadataResponse>>
{
    public async Task<ErrorOr<GetAllSettingsMetadataResponse>> Handle
        (GetAllSettingsMetadataQuery request, CancellationToken cancellationToken)
    {
        var settingsMetadata =
            await projection.GetAllSettingsMetadataAsync(cancellationToken);

        if (settingsMetadata.Count == 0)
            return Error.NotFound("No settings projections found");

        return CreateResponse(settingsMetadata);
    }

    private static GetAllSettingsMetadataResponse CreateResponse(IReadOnlyCollection<SettingsMetadata> domainEntities) =>
        new(domainEntities.Select(de => new SettingsMetadataResponse(de.ServiceName, de.EnvironmentNames)));
}
