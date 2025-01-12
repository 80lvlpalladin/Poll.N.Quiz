using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Poll.N.Quiz.Settings.Queries.Handlers;
using Poll.N.Quiz.API.Shared.Extensions;
using Poll.N.Quiz.Settings.Commands.Handlers;

namespace Poll.N.Quiz.Settings.API;

public static class Endpoints
{
    private const string BaseApiUrl = "api/v1/settings";

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var routeGroupBuilder = routeBuilder.MapGroup(BaseApiUrl);

        routeGroupBuilder.MapGet("/metadata", GetAllSettingsMetadataAsync)
            .WithDescription("Get all services and environments available");

        routeGroupBuilder.MapPost("", CreateSettingsAsync);
        routeGroupBuilder.MapPatch("", UpdateSettingsAsync);

        return routeBuilder;
    }

    private static Task<IResult> GetAllSettingsMetadataAsync
        (IMediator mediator, CancellationToken cancellationToken = default)
    {
        var query = new GetAllSettingsMetadataQuery();
        return mediator.SendAndReturnResultAsync(query, cancellationToken);
    }

    private static Task<IResult> CreateSettingsAsync(
        IMediator mediator,
        CreateOrUpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSettingsCommand
            (request.TimeStamp, request.ServiceName, request.EnvironmentName, request.JsonData);

        return mediator.SendAndReturnResultAsync(command, cancellationToken);
    }

    private static Task<IResult> UpdateSettingsAsync(
        IMediator mediator,
        CreateOrUpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSettingsCommand
            (request.TimeStamp, request.ServiceName, request.EnvironmentName, request.JsonData);

        return mediator.SendAndReturnResultAsync(command, cancellationToken);
    }

    private sealed record CreateOrUpdateSettingsRequest
        (uint TimeStamp, string ServiceName, string EnvironmentName, string JsonData);
}


