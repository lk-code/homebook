using HomeBook.Backend.Handler;

namespace HomeBook.Backend.Endpoints;

public static class SetupEndpoints
{
    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/setup")
            .WithTags("version");

        group.MapGet("/availability", SetupHandler.HandleGetAvailability)
            .WithName("GetAvailability")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "returns the status of the setup availability"
            })
            // HTTP 200 => Setup is not executed yet and available
            .Produces(StatusCodes.Status200OK)
            // HTTP 409 => Setup is already executed and not available
            .Produces(StatusCodes.Status409Conflict)
            // HTTP 500 => unknown error while setup checking
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
