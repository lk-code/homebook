using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.AspNetCore.Mvc;
using HomeBook.Backend.Responses;
using HomeBook.Backend.Requests;

namespace HomeBook.Backend.Handler;

public static class StorageFileHandler
{
    // GET - Read file content
    public static async Task<IResult> HandleGetFile([FromQuery] string filename,
        [FromQuery] Guid scopeId,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            return TypedResults.UnprocessableEntity("scope id is empty");

        bool isScopeRegistered = await storageProvider.IsScopeRegisteredAsync(scopeId, cancellationToken);
        if (!isScopeRegistered)
            return TypedResults.UnprocessableEntity();

        try
        {
            byte[] content = await storageProvider.GetFileAllBytesAsync(scopeId,
                filename,
                cancellationToken);

            return TypedResults.Ok(new FileGetResponse(filename,
                scopeId,
                content));
        }
        catch (Exception err)
        {
            // TODO: log and return error
            return TypedResults.Problem();
        }
    }

    // POST - Create/Update file
    public static async Task<IResult> HandlePostFile([FromBody] FilePostRequest request,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (request.ScopeId == Guid.Empty)
            return TypedResults.UnprocessableEntity("scope id is empty");

        bool isScopeRegistered = await storageProvider.IsScopeRegisteredAsync(request.ScopeId, cancellationToken);
        if (!isScopeRegistered)
            return TypedResults.UnprocessableEntity();

        try
        {
            await storageProvider.WriteFileAllBytesAsync(request.ScopeId,
                request.Filename,
                request.Content,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            // TODO: log and return error
            return TypedResults.Problem();
        }
    }

    // DELETE - Delete file
    public static async Task<IResult> HandleDeleteFile([FromQuery] string filename,
        [FromQuery] Guid scopeId,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            return TypedResults.UnprocessableEntity("scope id is empty");

        bool isScopeRegistered = await storageProvider.IsScopeRegisteredAsync(scopeId, cancellationToken);
        if (!isScopeRegistered)
            return TypedResults.UnprocessableEntity("scope is not registered");

        try
        {
            await storageProvider.DeleteFileAsync(scopeId,
                filename,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            // TODO: log and return error
            return TypedResults.Problem();
        }
    }
}
