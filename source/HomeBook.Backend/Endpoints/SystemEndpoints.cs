using HomeBook.Backend.Attributes;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;
using HomeBook.Backend.Middleware;

namespace HomeBook.Backend.Endpoints;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapSystemUsersEndpoints()
            .MapSystemInstanceEndpoints()
            .MapSystemStorageEndpoints();

        return routeBuilder;
    }

    private static IEndpointRouteBuilder MapSystemStorageEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/system/storage")
            .WithDescription("Endpoints for system storage management");

        group.MapGet("/info", SystemHandler.HandleGetSystemStorageInfo)
            .WithName("GetSystemStorageInfo")
            .WithTags("System", "System/Storage", "Require Admin")
            .WithDescription("returns several system informations (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<GetSystemStorageInfoResponse>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }

    private static IEndpointRouteBuilder MapSystemUsersEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/system/users")
            .WithDescription("Endpoints for user management");

        group.MapGet("/", SystemHandler.HandleGetUsers)
            .WithName("GetUsers")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Returns all users with pagination, optionally filtered by username (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<UsersResponse>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPost("/", SystemHandler.HandleCreateUser)
            .WithName("CreateUser")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Creates a new user (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<CreateUserResponse>()
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{userId:guid}", SystemHandler.HandleGetUserById)
            .WithName("GetUserById")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Returns a user by its id (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<UserResponse>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{userId:guid}", SystemHandler.HandleDeleteUser)
            .WithName("DeleteUser")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Deletes a user (Admin only, cannot delete self)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{userId:guid}/username", SystemHandler.HandleUpdateUsername)
            .WithName("UpdateUsername")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Updates a user's username (Admin only, checks for uniqueness ignoring case)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status409Conflict) // For username conflict
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{userId:guid}/password", SystemHandler.HandleUpdatePassword)
            .WithName("UpdateUserPassword")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Updates a user's password (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{userId:guid}/admin", SystemHandler.HandleUpdateUserAdmin)
            .WithName("UpdateUserAdmin")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Updates a user's admin status (Admin only, cannot change own status)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{userId:guid}/enable", SystemHandler.HandleEnableUser)
            .WithName("EnableUser")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Enables a disabled user (Admin only, cannot enable self)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{userId:guid}/disable", SystemHandler.HandleDisableUser)
            .WithName("DisableUser")
            .WithTags("System", "System/Users", "Require Admin")
            .WithDescription("Disables an active user (Admin only, cannot disable self)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }

    private static IEndpointRouteBuilder MapSystemInstanceEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/system/instance")
            .WithDescription("Endpoints for instance management");

        group.MapGet("/info", SystemHandler.HandleGetSystemInfo)
            .WithName("GetSystemInfo")
            .WithTags("System", "System/Instance", "Require Admin")
            .WithDescription("returns several system informations (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<GetSystemInfoResponse>()
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/name", SystemHandler.HandleUpdateInstanceName)
            .WithName("UpdateInstanceName")
            .WithTags("System", "System/Instance", "Require Admin")
            .WithDescription("Updates the instance name (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPut("/default-locale", SystemHandler.HandleUpdateInstanceDefaultLocale)
            .WithName("UpdateInstanceDefaultLocale")
            .WithTags("System", "System/Instance", "Require Admin")
            .WithDescription("Updates the instance default locale (Admin only)")
            .WithMetadata(new RequireAdminAttribute())
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status403Forbidden)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
