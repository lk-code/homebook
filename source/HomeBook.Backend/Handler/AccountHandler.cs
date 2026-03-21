using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using HomeBook.Backend.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HomeBook.Backend.Handler;

public class AccountHandler
{
    public static async Task<IResult> HandleLogin([FromBody] LoginRequest request,
        [FromServices] IAccountProvider accountProvider,
        CancellationToken cancellationToken,
        [FromServices] ILogger<AccountHandler> logger)
    {
        try
        {
            logger.LogInformation("Login attempt for user: {Username}", request.Username);

            JwtTokenResult? loginResult = await accountProvider.LoginAsync(
                request.Username,
                request.Password,
                cancellationToken);

            if (loginResult is null)
            {
                logger.LogWarning("Login failed for user: {Username}", request.Username);
                return TypedResults.Unauthorized();
            }

            logger.LogInformation("Login successful for user: {Username}", request.Username);

            LoginResponse response = new()
            {
                Token = loginResult.Token,
                RefreshToken = loginResult.RefreshToken,
                ExpiresAt = loginResult.ExpiresAt,
                UserId = loginResult.UserId,
                Username = loginResult.Username
            };

            return TypedResults.Ok(response);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation error during login for user: {Username}", request.Username);
            ValidationProblemDetails problemDetails = new()
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            };
            return TypedResults.BadRequest(problemDetails);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login for user: {Username}", request.Username);
            return TypedResults.Unauthorized();
        }
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> HandleLogout(
        [FromServices] IAccountProvider accountProvider,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        CancellationToken cancellationToken,
        [FromServices] ILogger<AccountHandler>? logger = null)
    {
        logger ??= NullLogger<AccountHandler>.Instance;
        try
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                logger.LogWarning("HttpContext is null during logout");
                return TypedResults.BadRequest("Invalid request context");
            }

            // Get the current user's token from the authorization header
            string? authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                logger.LogWarning("No valid authorization header found during logout");
                return TypedResults.BadRequest("No valid token provided");
            }

            string token = authHeader["Bearer ".Length..].Trim();

            logger.LogInformation("Processing logout request");

            bool success = await accountProvider.LogoutAsync(token, cancellationToken);

            if (!success)
            {
                logger.LogWarning("Logout request rejected");
                return TypedResults.BadRequest("Logout failed");
            }

            logger.LogInformation("Logout request completed successfully");
            return TypedResults.Ok("Logout successful");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during logout");
            return TypedResults.BadRequest("Logout failed");
        }
    }
}
