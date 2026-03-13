using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/user")
            .WithDescription("Endpoints for user management")
            .RequireAuthorization();

        group.MapGet("/preferences/locale", UserHandler.HandleGetUserPreferenceForLocale)
            .WithName("GetUserPreferenceForLocale")
            .WithTags("User")
            .WithDescription(new Description(
                "returns the user preference for locale",
                "HTTP 200: User preference was found",
                "HTTP 401: User is not authorized",
                "HTTP 500: Unknown error while getting preference"))
            .RequireAuthorization()
            .Produces<GetUserPreferenceLocaleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPost("/preferences/locale", UserHandler.HandleUpdateUserPreferenceForLocale)
            .WithName("UpdateUserPreferenceForLocale")
            .WithTags("User")
            .WithDescription(new Description("updates the user preference for locale",
                "HTTP 200: User preference was updated",
                "HTTP 401: User is not authorized",
                "HTTP 500: Unknown error while updating preference"))
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
