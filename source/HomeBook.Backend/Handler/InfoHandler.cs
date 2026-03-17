using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public class InfoHandler
{
    public static async Task<IResult> HandleGetInstanceInfo([FromServices] ILogger<InfoHandler> logger,
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string instanceName = await instanceConfigurationProvider
                .GetHomeBookInstanceNameAsync(cancellationToken);
            string? instanceDefaultName = await instanceConfigurationProvider
                .GetHomeBookInstanceDefaultLocaleAsync(cancellationToken);

            GetInstanceInfoResponse response = new(instanceName,
                (instanceDefaultName ?? string.Empty));
            return TypedResults.Ok(response);
        }
        catch (Exception err)
        {
            logger.LogError(err, "An error occurred while retrieving instance information.");

            return TypedResults.Problem("An error occurred while retrieving instance information.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleGetInstanceName([FromServices] ILogger<InfoHandler> logger,
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? instanceName = await instanceConfigurationProvider.GetHomeBookInstanceNameAsync(cancellationToken);

            return TypedResults.Ok(instanceName);
        }
        catch (Exception err)
        {
            logger.LogError(err, "An error occurred while retrieving instance information.");

            return TypedResults.Problem("An error occurred while retrieving instance name.", statusCode: 500);
        }
    }

    public static async Task<IResult> HandleGetInstanceDefaultLocale([FromServices] ILogger<InfoHandler> logger,
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? defaultLanguage =
                await instanceConfigurationProvider.GetHomeBookInstanceDefaultLocaleAsync(cancellationToken);

            return TypedResults.Ok(defaultLanguage);
        }
        catch (Exception err)
        {
            logger.LogError(err, "An error occurred while retrieving instance information.");

            return TypedResults.Problem("An error occurred while retrieving instance name.", statusCode: 500);
        }
    }
}
