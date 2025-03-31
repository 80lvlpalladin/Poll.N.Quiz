using ErrorOr;
using MediatR;
using Poll.N.Quiz.Settings.Domain.ValueObjects;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

namespace Poll.N.Quiz.Settings.Queries.Handlers;

public record GetAllSettingsMetadataQuery() : IRequest<ErrorOr<GetAllSettingsMetadataResponse>>;
public record GetAllSettingsMetadataResponse(IEnumerable<SettingsMetadataResponse> Metadata);
public record SettingsMetadataResponse(string ServiceName, IEnumerable<string> EnvironmentNames);

public class GetAllSettingsMetadataHandler(IReadOnlySettingsProjectionStore projection) :
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

    private static GetAllSettingsMetadataResponse CreateResponse
        (IReadOnlyCollection<SettingsMetadata> domainEntities)
    {
        var settingsMetadataResponses = domainEntities
            .GroupBy(entity => entity.ServiceName)
            .Select(group =>
                new SettingsMetadataResponse(
                    group.Key,
                    group.Select(metadata => metadata.EnvironmentName)));

        return new GetAllSettingsMetadataResponse(settingsMetadataResponses);
    }
}
