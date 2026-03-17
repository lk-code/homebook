using HomeBook.Backend.Handler;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/account")
            .WithDescription("Endpoints for Account Management");

        group.MapPost("/login", AccountHandler.HandleLogin)
            .WithName("Login")
            .WithTags("Account", "Authentication")
            .WithDescription("Authenticates a user with email and password, returns JWT tokens")
            .Accepts<LoginRequest>("application/json")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", AccountHandler.HandleLogout)
            .WithName("Logout")
            .WithTags("Account", "Authentication")
            .WithDescription("Invalidates the current user's access token")
            .RequireAuthorization()
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        return routeBuilder;
    }
}
