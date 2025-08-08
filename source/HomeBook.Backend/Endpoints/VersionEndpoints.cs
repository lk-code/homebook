using HomeBook.Backend.Handler;

namespace HomeBook.Backend.Endpoints;

public static class VersionEndpoints
{
    public static IEndpointRouteBuilder MapVersionEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/version")
            .WithTags("version");

        group.MapGet("/", VersionHandler.HandleGetVersion)
            .WithName("GetVersion")
            // .RequireAuthorization(policy => policy.RequireRole("read"))
            .WithOpenApi(operation => new(operation)
            {
                Summary = "returns the version of the backend service"
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
