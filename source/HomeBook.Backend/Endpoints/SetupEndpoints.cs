using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class SetupEndpoints
{
    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/setup")
            .WithDescription("Endpoints for setup management");

        group.MapGet("/availability", SetupHandler.HandleGetAvailability)
            .WithName("GetAvailability")
            .WithTags("Setup")
            .WithDescription(new Description("returns the status of the setup availability",
                "HTTP 200: Setup is not executed yet and available => Setup can be started",
                "HTTP 201: Setup is finished, but an update is required => Update must be executed before Homebook can be used",
                "HTTP 204: Setup is finished and no update is required => Homebook is ready to use",
                "HTTP 500: Unknown error while setup checking"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/licenses", SetupHandler.HandleGetLicenses)
            .WithName("GetLicenses")
            .WithTags("Setup")
            .WithDescription(new Description("returns all licenses of the project",
                "HTTP 200: Licenses found",
                "HTTP 500: Unknown error while loading licenses"))
            .Produces<GetLicensesResponse>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/database/configuration", SetupHandler.HandleGetDatabaseCheck)
            .WithName("GetDatabaseCheck")
            .WithTags("Setup")
            .WithDescription(new Description("check if database configuration is available via environment variables",
                "HTTP 200: Database configuration found",
                "HTTP 400: Validation error, e.g. too short password, etc.",
                "HTTP 404: No Database configuration found",
                "HTTP 500: Unknown error while checking Database configuration"))
            .Produces<GetDatabaseCheckResponse>(StatusCodes.Status200OK)
            .Produces<string[]>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPost("/database/check", SetupHandler.HandleCheckDatabase)
            .WithName("CheckDatabase")
            .WithTags("Setup")
            .WithDescription(new Description("check that the Database is available",
                "HTTP 200: Database is available => returns the detected database provider in uppercases",
                "HTTP 500: Unknown error while database connection check",
                "HTTP 503: Database is not available"))
            .Accepts<CheckDatabaseRequest>("application/json")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/user", SetupHandler.HandleGetPreConfiguredUser)
            .WithName("GetPreConfiguredUser")
            .WithTags("Setup")
            .WithDescription(new Description("check if a pre-configured user is available via environment variables",
                "HTTP 200: A user was pre-configured",
                "HTTP 404: No pre-configured user found",
                "HTTP 500: Unknown error while loading pre-configured user"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapGet("/configuration", SetupHandler.HandleGetConfiguration)
            .WithName("GetConfiguration")
            .WithTags("Setup")
            .WithDescription(new Description(
                "returns the homebook setup configuration if it is set via environment variables.",
                "HTTP 200: Configuration was found",
                "HTTP 404: No configuration found",
                "HTTP 500: Unknown error while loading configuration"))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        group.MapPost("/start", SetupHandler.HandleStartSetup)
            .WithName("StartSetup")
            .WithTags("Setup")
            .WithDescription(new Description(
                "start the setup process. it will save the configuration for the setup steps",
                "HTTP 200: Setup started successfully",
                "HTTP 400: Validation error for example with the database configuration, e.g. too short password, etc.",
                "HTTP 422: Licenses not accepted",
                "HTTP 500: Unknown error while starting setup"))
            .Accepts<StartSetupRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status422UnprocessableEntity)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
