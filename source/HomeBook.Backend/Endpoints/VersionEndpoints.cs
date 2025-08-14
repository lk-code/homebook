using HomeBook.Backend.Handler;
using Microsoft.OpenApi.Models;

namespace HomeBook.Backend.Endpoints;

public static class VersionEndpoints
{
    public static IEndpointRouteBuilder MapVersionEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/version")
            .WithTags("version")
            .WithDescription("Endpoints for application version");

        group.MapGet("/", VersionHandler.HandleGetVersion)
            .WithName("GetVersion")
            .WithDescription("returns the version of the backend service")
            // .RequireAuthorization(policy => policy.RequireRole("read"))
            .WithOpenApi(operation => new(operation)
            {
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
