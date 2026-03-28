using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class DevelopmentEndpoints
{
    public static IEndpointRouteBuilder MapDevelopmentEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/Development")
            .WithDescription("Endpoints for development and diagnostics");

        group.MapGet("/Config", DevelopmentHandler.HandleGetConfig)
            .WithName("GetDevelopmentConfig")
            .WithTags("Development")
            .WithDescription("Returns all configuration values currently present in IConfiguration")
            .RequireAuthorization()
            .Produces<GetDevelopmentConfigResponse>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
