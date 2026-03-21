using HomeBook.Backend.Mappings;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Handler;

public class PlatformHandler
{
    public static IResult HandleGetLocales([FromServices] ILogger<PlatformHandler> logger,
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            var availableLocales = new List<string>
            {
                "en-GB",
                "en-US",
                "de-DE",
                "fr-FR"
            };

            LocaleResponse[] localeResponse = availableLocales
                .Select(x => x.ToLocalResponse())
                .OrderBy(x => x.Name)
                .ToArray();
            GetLocalesResponse response = new(localeResponse);

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while loading available locales");
            return TypedResults.Problem("An error occurred while loading available locales", statusCode: 500);
        }
    }
}
