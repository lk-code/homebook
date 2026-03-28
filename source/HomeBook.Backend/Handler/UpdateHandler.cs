using HomeBook.Backend.Abstractions.Contracts;
using Homebook.Backend.Core.Setup.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Handler;

public class UpdateHandler
{
    /// <summary>
    /// starts the homebook update process.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="setupInstanceManager"></param>
    /// <param name="hostApplicationLifetime"></param>
    /// <param name="updateProcessor"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleStartUpdate([FromServices] ISetupInstanceManager setupInstanceManager,
        [FromServices] IHostApplicationLifetime hostApplicationLifetime,
        [FromServices] IUpdateProcessor updateProcessor,
        CancellationToken cancellationToken,
        [FromServices] ILogger<UpdateHandler>? logger = null)
    {
        logger ??= NullLogger<UpdateHandler>.Instance;
        try
        {
            bool setupFinished = setupInstanceManager.IsHomebookInstanceCreated();
            bool updateRequired = await setupInstanceManager.IsUpdateRequiredAsync(cancellationToken);

            if (!setupFinished)
                // HTTP 409 => Setup was not executed yet
                return TypedResults.Conflict();

            if (!updateRequired)
                // HTTP 200 => no update available
                return TypedResults.Ok();

            // 1. execute update
            await updateProcessor.ProcessAsync(cancellationToken);

            // 2. restart service
            hostApplicationLifetime.StopApplication();

            // HTTP 200 => update finished
            return TypedResults.Ok();
        }
        catch (SetupException err)
        {
            logger.LogError(err, "Error while updating");
            return TypedResults.InternalServerError(err.Message);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating");
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
