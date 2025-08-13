using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Setup;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public static class SetupHandler
{
    public static async Task<Results<Ok, Conflict, InternalServerError<string>>> HandleGetAvailability([FromServices] ISetupInstanceManager setupInstanceManager,
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
            return TypedResults.InternalServerError(err.Message);
        }
    }

    public static async Task<Results<Ok<GetDatabaseCheckResponse>, NotFound, InternalServerError<string>>> HandleGetDatabaseCheck([FromServices] IFileService fileService,
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

            GetDatabaseCheckResponse response = new(databaseHost,
                databasePort,
                databaseName,
                databaseUserName,
                databaseUserPassword);

            bool databaseConfigurationFound = databaseHost is not null
                                              && databasePort is not null
                                              && databaseName is not null
                                              && databaseUserName is not null
                                              && databaseUserPassword is not null;
            if (databaseConfigurationFound)
                return TypedResults.Ok(response);
            else
                return TypedResults.NotFound();
        }
        catch (Exception err)
        {
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
