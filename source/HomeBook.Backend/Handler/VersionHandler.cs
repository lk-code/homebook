using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public static class VersionHandler
{
    // public static async Task<Results<Ok<AnalysisResponse>, BadRequest<string>, NotImplementedResult>> HandleGetVersion(
    //     [FromQuery] int? maxResults,
    //     [FromBody] AnalysisRequest request,
    //     [FromServices] IValidator<AnalysisRequest> validator,
    //     [FromServices] IAnalysisService analysisService,
    //     [FromServices] IConfiguration configuration,
    //     CancellationToken cancellationToken)
    // {
    //
    // }

    public static async Task<Results<Ok<string>, InternalServerError<string>>> HandleGetVersion(
        [FromServices] IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        string? version = configuration.GetSection("Version").Value;

        if (string.IsNullOrEmpty(version))
        {
            return TypedResults.InternalServerError("Service Version is not configured.");
        }

        return TypedResults.Ok(version);
    }
}
