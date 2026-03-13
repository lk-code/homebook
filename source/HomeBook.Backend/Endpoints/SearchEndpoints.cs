using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.DTOs.Responses.Search;
using HomeBook.Backend.Handler;

namespace HomeBook.Backend.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/search")
            .WithDescription("Endpoints for search functionality")
            .RequireAuthorization();

        group.MapGet("/", SearchHandler.HandleSearch)
            .WithName("Search")
            .WithTags("Search")
            .WithDescription(new Description(
                "returns search results based on the query",
                "HTTP 200: Search results were found",
                "HTTP 401: User is not authorized",
                "HTTP 500: Unknown error while getting preference"))
            .RequireAuthorization()
            .Produces<SearchResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
