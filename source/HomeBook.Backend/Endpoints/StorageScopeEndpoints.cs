using HomeBook.Backend.Handler;

namespace HomeBook.Backend.Endpoints;

public static class StorageScopeEndpoints
{
    public static IEndpointRouteBuilder MapStorageScopeEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/storage/scopes")
            .WithDescription("Endpoints for storage operations")
            .RequireAuthorization();

        // GET - Get scope ID by scope name
        group.MapGet("/", StorageScopeHandler.HandleGetScopeIdByName)
            .WithName("GetScopeIdByName")
            .WithTags("Storage")
            .WithDescription("Returns the scope ID for a given scope name")
            .RequireAuthorization()
            .Produces<Guid>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
