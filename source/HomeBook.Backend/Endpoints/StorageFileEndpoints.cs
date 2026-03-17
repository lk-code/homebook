using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.DTOs.Responses.Storage;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class StorageFileEndpoints
{
    public static IEndpointRouteBuilder MapStorageFileEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/storage/files")
            .WithDescription("Endpoints for storage operations")
            .RequireAuthorization();

        // GET - get file as asset
        group.MapGet("/{mediaId:guid}", StorageFileHandler.HandleGetFileByIdMedia)
            .WithName("HGetFileByIdMedia")
            .WithTags("Storage")
            .WithDescription(new Description("get the file as assets content",
                "HTTP 200: File content returned successfully",
                "HTTP 404: File not found"))
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status404NotFound);

        // GET - Read file content
        group.MapGet("", StorageFileHandler.HandleGetFile)
            .WithName("GetFile")
            .WithTags("Storage")
            .WithDescription(new Description("Reads file content by filename and scope",
                "HTTP 200: File content returned successfully",
                "HTTP 400: Invalid parameters",
                "HTTP 404: File not found",
                "HTTP 422: Storage Scope not found",
                "HTTP 500: Unknown error while reading file"))
            .RequireAuthorization()
            .Produces<FileGetResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        // POST - Create/Update file
        group.MapPost("", StorageFileHandler.HandlePostFile)
            .WithName("PostFile")
            .WithTags("Storage")
            .WithDescription(new Description("Creates or updates a file with binary content",
                "HTTP 200: File created/updated successfully",
                "HTTP 400: Invalid parameters or content",
                "HTTP 422: Storage Scope not found",
                "HTTP 500: Unknown error while writing file"))
            .RequireAuthorization()
            .Produces<FilePostResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        // DELETE - Delete file
        group.MapDelete("", StorageFileHandler.HandleDeleteFile)
            .WithName("DeleteFile")
            .WithTags("Storage")
            .WithDescription(new Description("Deletes a file by filename and scope",
                "HTTP 200: File deleted successfully",
                "HTTP 400: Invalid parameters",
                "HTTP 422: Storage Scope not found",
                "HTTP 500: Unknown error while deleting file"))
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
