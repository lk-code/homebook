using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class PlatformEndpoints
{
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/platform")
            .WithDescription("Endpoints for platform management");

        group.MapGet("/locales", PlatformHandler.HandleGetLocales)
            .WithName("GetLocales")
            .WithTags("Platform")
            .WithDescription("returns all available locales")
            .Produces<GetLocalesResponse>( StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
