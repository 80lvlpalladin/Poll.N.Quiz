using MediatR;
using ErrorOr;
using Poll.N.Quiz.Settings.ProjectionStore.ReadOnly;

namespace Poll.N.Quiz.Settings.API.Queries.Handlers;

public sealed record GetSettingsMetadataQuery(string? ServiceName = null) :
    IRequest<ErrorOr<GetSettingsMetadataResponse>>;
public sealed record GetSettingsMetadataResponse(IEnumerable<ServiceMetadataResponse> ServiceMetadata);
public sealed record ServiceMetadataResponse(string ServiceName, IEnumerable<string> EnvironmentNames);


public class GetSettingsMetadataHandler(IReadOnlySettingsProjectionStore projection) :
    IRequestHandler<GetSettingsMetadataQuery, ErrorOr<GetSettingsMetadataResponse>>
{
    public async Task<ErrorOr<GetSettingsMetadataResponse>> Handle(GetSettingsMetadataQuery request, CancellationToken cancellationToken)
    {
        var domainSettingsMetadata =
            await projection.GetSettingsMetadataAsync(request.ServiceName, cancellationToken);

        if (domainSettingsMetadata.Count == 0)
            return Error.NotFound("No settings projections found");

        var serviceMetadataResponses = domainSettingsMetadata
             .GroupBy(entity => entity.ServiceName)
             .Select(group =>
                 new ServiceMetadataResponse(
                     group.Key,
                     group.Select(metadata => metadata.EnvironmentName)));

         return new GetSettingsMetadataResponse(serviceMetadataResponses);
    }
}
