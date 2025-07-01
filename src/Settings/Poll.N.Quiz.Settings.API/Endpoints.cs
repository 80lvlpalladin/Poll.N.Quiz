using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Poll.N.Quiz.Settings.API.Queries.Handlers;
using Poll.N.Quiz.API.Shared.Extensions;
using Poll.N.Quiz.Settings.API.Commands.Handlers;
using Poll.N.Quiz.Settings.API.Synchronizer.Handlers;

namespace Poll.N.Quiz.Settings.API;

public static class Endpoints
{
    private const string BaseApiUrl = "api/v1/settings";

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var routeGroupBuilder = routeBuilder.MapGroup(BaseApiUrl);

        routeGroupBuilder.MapGet("/metadata", GetAllSettingsMetadataAsync)
            .WithDescription("Get all services and environments available");

        routeGroupBuilder.MapPost("/reload-projection", ReloadProjectionAsync)
            .WithDescription("Reload settings projection for a specific service and environment");

        routeGroupBuilder.MapPost("", CreateSettingsAsync);
        routeGroupBuilder.MapPatch("", UpdateSettingsAsync);
        routeGroupBuilder.MapGet("/{serviceName}/{environmentName}", GetSettingsContentAsync);

        return routeBuilder;
    }

    private static Task<IResult> GetSettingsContentAsync(
        IMediator mediator,
        string serviceName,
        string environmentName,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSettingsContentQuery(serviceName, environmentName);
        return mediator.SendAndReturnResultAsync(query, cancellationToken);
    }

    private static Task<IResult> ReloadProjectionAsync(
        IMediator mediator,
        [FromBody] ReloadProjectionWebRequest webRequest,
        CancellationToken cancellationToken = default)
    {
        var request = new ReloadProjectionRequest(webRequest.ServiceName, webRequest.EnvironmentName);
        return mediator.SendAndReturnResultAsync(request, cancellationToken);
    }

    private static Task<IResult> GetAllSettingsMetadataAsync
        (IMediator mediator, CancellationToken cancellationToken = default)
    {
        var query = new GetAllSettingsMetadataQuery();
        return mediator.SendAndReturnResultAsync(query, cancellationToken);
    }

    private static Task<IResult> CreateSettingsAsync(
        IMediator mediator,
        [FromBody] CreateSettingsWebRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSettingsCommand(
            request.TimeStamp,
            request.Version,
            request.ServiceName,
            request.EnvironmentName,
            request.SettingsJson);

        return mediator.SendAndReturnResultAsync(command, cancellationToken);
    }

    private static Task<IResult> UpdateSettingsAsync(
        IMediator mediator,
        [FromBody] UpdateSettingsWebRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSettingsCommand(
            request.TimeStamp,
            request.Version,
            request.ServiceName,
            request.EnvironmentName,
            request.SettingsPatchJson);

        return mediator.SendAndReturnResultAsync(command, cancellationToken);
    }

    private sealed record CreateSettingsWebRequest(
        uint TimeStamp,
        uint Version,
        string ServiceName,
        string EnvironmentName,
        string SettingsJson);

    private sealed record UpdateSettingsWebRequest(
        uint TimeStamp,
        uint Version,
        string ServiceName,
        string EnvironmentName,
        string SettingsPatchJson);

    private sealed record ReloadProjectionWebRequest(
        string ServiceName,
        string EnvironmentName);

    private sealed record GetSettingsContentWebRequest(
        string ServiceName,
        string EnvironmentName);
}


