using HomeBook.Backend.Abstractions;
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
            // check if file "/var/lib/homebook/instance.txt" exists
            string instanceFilePath = "/var/lib/homebook/instance.txt";
            bool instanceFileExists = await fileService.DoesFileExistsAsync(instanceFilePath); // false => means setup is not executed yet and available

            if (!instanceFileExists)
                // does not exist => setup is not executed yet and available
                return TypedResults.Ok();
            else
                // exists => setup is already executed and not available
                return TypedResults.Conflict();
        }
        catch (Exception err)
        {
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
