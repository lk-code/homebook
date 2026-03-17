using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;
using HomeBook.Backend.Middleware;

namespace HomeBook.Backend.Endpoints;

public static class InfoEndpoints
{
    public static IEndpointRouteBuilder MapInfoEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/info")
            .WithDescription("Endpoints for instance information");

        group.MapGet("/", InfoHandler.HandleGetInstanceInfo)
            .WithName("GetInstanceInfo")
            .WithTags("Info")
            .WithDescription("Returns instance information (requires authentication)")
            .RequireAuthorization()
            .Produces<GetInstanceInfoResponse>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/name", InfoHandler.HandleGetInstanceName)
            .WithName("GetInstanceName")
            .WithTags("Info")
            .WithDescription("Returns the instance name as a string (public endpoint)")
            .AllowAnonymous()
            .Produces<string>()
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/default-locale", InfoHandler.HandleGetInstanceDefaultLocale)
            .WithName("GetInstanceDefaultLocale")
            .WithTags("Info")
            .WithDescription("Returns the instance default locale as a string (public endpoint) e.g. 'en-EN', 'de-DE' ...")
            .AllowAnonymous()
            .Produces<string>()
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
