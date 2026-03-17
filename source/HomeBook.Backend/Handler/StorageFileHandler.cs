using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.Media;
using HomeBook.Backend.DTOs.Responses.Storage;
using Microsoft.AspNetCore.Mvc;
using HomeBook.Backend.Requests;
using Microsoft.AspNetCore.StaticFiles;

namespace HomeBook.Backend.Handler;

public static class StorageFileHandler
{
    // GET - Read file content
    public static async Task<IResult> HandleGetFileByIdMedia(Guid mediaId,
        [FromServices] IMediaProvider mediaProvider,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (mediaId == Guid.Empty)
            return TypedResults.NotFound();

        MediaItemDto? mediaItem = await mediaProvider.GetMediaItemByIdAsync(mediaId,
            cancellationToken);
        if (mediaItem is null)
            return TypedResults.NotFound();

        bool isScopeRegistered = await storageProvider.IsScopeRegisteredAsync(mediaItem.ScopeId,
            cancellationToken);
        if (!isScopeRegistered)
            return TypedResults.NotFound();

        try
        {
            byte[] content = await storageProvider.GetFileAllBytesAsync(mediaItem.ScopeId,
                mediaItem.Filename,
                cancellationToken);

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(mediaItem.Filename, out string? contentType))
                contentType = "application/octet-stream";

            return TypedResults.File(content, contentType);
        }
        catch (Exception err)
        {
            // TODO: log and return error
            return TypedResults.Problem();
        }
    }

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
            Guid mediaItemId = await storageProvider.WriteFileAllBytesAsync(request.ScopeId,
                request.Filename,
                request.Content,
                cancellationToken);

            return TypedResults.Ok(new FilePostResponse(mediaItemId));
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
