using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Handler;

public class StorageScopeHandler
{
    // GET - Get scope ID by scope name
    public static async Task<IResult> HandleGetScopeIdByName([FromQuery] string name,
        [FromServices] ILogger<StorageScopeHandler> logger,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return TypedResults.BadRequest("Scope name is required");

        try
        {
            Guid? scopeId = await storageProvider.GetScopeIdByFullNameAsync(name, cancellationToken);

            if (scopeId == null)
                return TypedResults.NotFound("Scope not found");

            return TypedResults.Ok(scopeId.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while retrieving storage scope identifier");
            return TypedResults.Problem("An error occurred while retrieving the scope ID", statusCode: 500);
        }
    }
}
