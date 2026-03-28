using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Handler;

public class VersionHandler
{
    public static IResult HandleGetVersion(
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken,
        [FromServices] ILogger<VersionHandler>? logger = null)
    {
        logger ??= NullLogger<VersionHandler>.Instance;
        string? version = configuration.GetSection("Version")?.Value?.Trim();

        if (string.IsNullOrEmpty(version))
        {
            logger.LogError("Service version is not configured");
            return TypedResults.InternalServerError("Service Version is not configured.");
        }

        return TypedResults.Ok(version);
    }
}
