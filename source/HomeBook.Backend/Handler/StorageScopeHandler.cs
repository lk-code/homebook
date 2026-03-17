using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public static class StorageScopeHandler
{
    // GET - Get scope ID by scope name
    public static async Task<IResult> HandleGetScopeIdByName([FromQuery] string name,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return TypedResults.BadRequest("Scope name is required");

        try
        {
            Guid? scopeId = await storageProvider.GetScopeIdByFullName(name, cancellationToken);

            if (scopeId == null)
                return TypedResults.NotFound("Scope not found");

            return TypedResults.Ok(scopeId.Value);
        }
        catch (Exception)
        {
            // TODO: log and return error
            return TypedResults.Problem("An error occurred while retrieving the scope ID", statusCode: 500);
        }
    }
}
