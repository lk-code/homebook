using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public static class SetupHandler
{
    public static async Task<Results<Ok, Conflict, InternalServerError<string>>> HandleGetAvailability(
        [FromServices] IFileService fileService,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. check if file "/var/lib/homebook/instance.txt" exists
            string instanceFilePath = "/var/lib/homebook/instance.txt";
            bool instanceFileExists = await fileService.DoesFileExistsAsync(instanceFilePath); // false => means setup is not executed yet and available

            if (!instanceFileExists)
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

    public static async Task<Results<Ok<GetDatabaseCheckResponse>, NotFound, InternalServerError<string>>> HandleGetDatabaseCheck(
        [FromServices] IFileService fileService,
        CancellationToken cancellationToken)
    {
        try
        {
            GetDatabaseCheckResponse response = new("homebook-db", "dbadmin", "a-sample-password");

            bool databaseConfigurationFound = true;
            if (!databaseConfigurationFound)
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
