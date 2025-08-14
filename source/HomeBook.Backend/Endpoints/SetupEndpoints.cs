using HomeBook.Backend.Handler;
using HomeBook.Backend.OpenApi;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class SetupEndpoints
{
    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/setup")
            .WithTags("setup")
            .WithDescription("Endpoints for setup management");

        group.MapGet("/availability", SetupHandler.HandleGetAvailability)
            .WithName("GetAvailability")
            .WithDescription(new Description("returns the status of the setup availability",
                "HTTP 200: Setup is not executed yet and available",
                "HTTP 409: Setup is already executed and not available",
                "HTTP 500: Unknown error while setup checking"))
            .WithOpenApi(operation => new(operation)
            {
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/database/configuration", SetupHandler.HandleGetDatabaseCheck)
            .WithName("GetDatabaseCheck")
            .WithDescription(new Description("check if database configuration is available via environment variables",
                "HTTP 200: Database configuration found",
                "HTTP 400: Validation error, e.g. too short password, etc.",
                "HTTP 404: No Database configuration found",
                "HTTP 500: Unknown error while setup checking"))
            .WithOpenApi(operation => new(operation)
            {
                // Summary = "check if database configuration is available via environment variables"
            })
            .Produces<GetDatabaseCheckResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPost("/database/check", SetupHandler.HandleCheckDatabase)
            .WithName("CheckDatabase")
            .WithDescription(new Description("check that the Database is available",
                "HTTP 200: Database is available",
                "HTTP 503: Database is not available",
                "HTTP 500: Unknown error while database connection check"))
            .WithOpenApi(operation => new(operation)
            {
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPost("/database/migrate", SetupHandler.HandleMigrateDatabase)
            .WithName("MigrateDatabase")
            .WithDescription(new Description("migrate the database schema to the latest version",
                "HTTP 200: Database migration was successful",
                "HTTP 409: Database migration is already executed and not available",
                "HTTP 500: Unknown error while database migration"))
            .WithOpenApi(operation => new(operation)
            {
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
