using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.Handler;

namespace HomeBook.Backend.Endpoints;

public static class UpdateEndpoints
{
    public static IEndpointRouteBuilder MapUpdateEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/update")
            .WithDescription("Endpoints for setup management");

        group.MapPost("/start", UpdateHandler.HandleStartUpdate)
            .WithName("Update")
            .WithTags("Update")
            .WithDescription(new Description("start the update process",
                "HTTP 200: Update was successful",
                "HTTP 409: Setup was not executed yet - setup must be completed before update can be started",
                "HTTP 500: Unknown error while starting update"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces<string>(StatusCodes.Status500InternalServerError);


        return routeBuilder;
    }
}
