using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public class DevelopmentHandler
{
    public static async Task<IResult> HandleGetConfig(
        [FromServices] ILogger<DevelopmentHandler> logger,
        [FromServices] IDevelopmentConfigProvider developmentConfigProvider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<KeyValuePair<string, string?>> values = await developmentConfigProvider
                .GetConfigurationValuesAsync(cancellationToken);

            GetDevelopmentConfigResponse response = new(values);
            return TypedResults.Ok(response);
        }
        catch (Exception err)
        {
            logger.LogError(err, "An error occurred while retrieving configuration values");
            return TypedResults.Problem("An error occurred while retrieving configuration values.", statusCode: 500);
        }
    }
}
