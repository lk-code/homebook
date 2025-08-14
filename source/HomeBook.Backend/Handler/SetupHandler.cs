using FluentValidation;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Setup;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public class SetupHandler
{
    /// <summary>
    /// checks if the setup is available and no setup instance is created yet.
    /// </summary>
    /// <param name="setupInstanceManager"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetAvailability([FromServices] ILogger<SetupHandler> logger,
        [FromServices] ISetupInstanceManager setupInstanceManager,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. check if file "/var/lib/homebook/instance.txt" exists
            bool setupInstanceExists = await setupInstanceManager.IsSetupInstanceCreatedAsync(cancellationToken);

            if (!setupInstanceExists)
                // does not exist => setup is not executed yet and available
                return TypedResults.Ok();
            else
                // exists => setup is already executed and not available
                return TypedResults.Conflict();

            // 2. check dependencies like Redis, etc.
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while checking setup availability");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// returns the database configuration if it is available (set via environment variables).
    /// </summary>
    /// <param name="fileService"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetDatabaseCheck([FromServices] ILogger<SetupHandler> logger,
        [FromServices] IFileService fileService,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            string? databaseHost = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_HOST);
            string? databasePort = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PORT);
            string? databaseName = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_NAME);
            string? databaseUserName = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_USER);
            string? databaseUserPassword = setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PASSWORD);


            bool databaseConfigurationFound = databaseHost is not null
                                              && databasePort is not null
                                              && databaseName is not null
                                              && databaseUserName is not null
                                              && databaseUserPassword is not null;
            if (!databaseConfigurationFound)
                return TypedResults.NotFound();

            GetDatabaseCheckResponse response = new(databaseHost,
                databasePort,
                databaseName,
                databaseUserName,
                databaseUserPassword);
            return TypedResults.Ok(response);
        }
        catch (ValidationException err)
        {
            logger.LogError(err, "Validation error while getting database configuration");
            return TypedResults.BadRequest(err.Errors.Select(x => x.ErrorMessage).ToArray());
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while checking database configuration");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// check the database connection via the given configuration.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="databaseManager"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleCheckDatabase([FromBody] CheckDatabaseRequest request,
        [FromServices] ILogger<SetupHandler> logger,
        [FromServices] IDatabaseManager databaseManager,
        CancellationToken cancellationToken)
    {
        try
        {
            bool isDatabaseAvailable = await databaseManager.IsDatabaseAvailableAsync(
                request.DatabaseHost,
                request.DatabasePort,
                request.DatabaseName,
                request.DatabaseUserName,
                request.DatabaseUserPassword,
                cancellationToken);

            if (isDatabaseAvailable)
                // database is available
                return TypedResults.Ok();
            else
                // database is not available
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while checking database connection");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// starts the database migration process.
    /// </summary>
    /// <param name="fileService"></param>
    /// <param name="setupConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleMigrateDatabase([FromServices] ILogger<SetupHandler> logger,
        [FromServices] IFileService fileService,
        [FromServices] ISetupConfigurationProvider setupConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            return TypedResults.Ok();
            return TypedResults.StatusCode(StatusCodes.Status409Conflict);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while migrating database");
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
