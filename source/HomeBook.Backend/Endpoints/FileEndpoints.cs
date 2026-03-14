using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/file")
            .WithDescription("Endpoints for file operations")
            .RequireAuthorization();

        // GET - Read file content
        group.MapGet("", FileHandler.HandleGetFile)
            .WithName("GetFile")
            .WithTags("File")
            .WithDescription(new Description("Reads file content by filename and scope",
                "HTTP 200: File content returned successfully",
                "HTTP 400: Invalid parameters",
                "HTTP 404: File not found",
                "HTTP 500: Unknown error while reading file"))
            .RequireAuthorization()
            .Produces<FileGetResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        // POST - Create/Update file
        group.MapPost("", FileHandler.HandlePostFile)
            .WithName("PostFile")
            .WithTags("File")
            .WithDescription(new Description("Creates or updates a file with binary content",
                "HTTP 200: File created/updated successfully",
                "HTTP 400: Invalid parameters or content",
                "HTTP 500: Unknown error while writing file"))
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        // DELETE - Delete file
        group.MapDelete("", FileHandler.HandleDeleteFile)
            .WithName("DeleteFile")
            .WithTags("File")
            .WithDescription(new Description("Deletes a file by filename and scope",
                "HTTP 200: File deleted successfully",
                "HTTP 400: Invalid parameters",
                "HTTP 404: File not found",
                "HTTP 500: Unknown error while deleting file"))
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}