using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.Media;
using HomeBook.Backend.DTOs.Responses.Storage;
using Microsoft.AspNetCore.Mvc;
using HomeBook.Backend.Requests;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Handler;

public class StorageFileHandler
{
    // GET - Read file content
    public static async Task<IResult> HandleGetFileByIdMedia(Guid mediaId,
        [FromServices] ILogger<StorageFileHandler> logger,
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
            logger.LogError(err, "Error while retrieving media file");
            return TypedResults.Problem();
        }
    }

    // GET - Read file content
    public static async Task<IResult> HandleGetFile([FromQuery] string filename,
        [FromQuery] Guid scopeId,
        [FromServices] ILogger<StorageFileHandler> logger,
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
            logger.LogError(err, "Error while retrieving file");
            return TypedResults.Problem();
        }
    }

    // POST - Create/Update file
    public static async Task<IResult> HandlePostFile([FromBody] FilePostRequest request,
        [FromServices] ILogger<StorageFileHandler> logger,
        [FromServices] IStorageProvider storageProvider,
        CancellationToken cancellationToken)
    {
        if (request.ScopeId == Guid.Empty)
        {
            logger.LogError("ScopeId is empty");
            return TypedResults.UnprocessableEntity("scope id is empty");
        }

        bool isScopeRegistered = await storageProvider.IsScopeRegisteredAsync(request.ScopeId, cancellationToken);
        if (!isScopeRegistered)
        {
            logger.LogError("No scope was found for this ScopeId");
            return TypedResults.UnprocessableEntity();
        }

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
            logger.LogError(err, "Error while writing file");
            return TypedResults.Problem();
        }
    }

    // DELETE - Delete file
    public static async Task<IResult> HandleDeleteFile([FromQuery] string filename,
        [FromQuery] Guid scopeId,
        [FromServices] ILogger<StorageFileHandler> logger,
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
            logger.LogError(err, "Error while deleting file");
            return TypedResults.Problem();
        }
    }
}
