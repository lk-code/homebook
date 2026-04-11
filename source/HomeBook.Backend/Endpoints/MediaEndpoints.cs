using HomeBook.Backend.Core.Modules.OpenApi;
using HomeBook.Backend.DTOs.Responses.Media;
using HomeBook.Backend.DTOs.Responses.Storage;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder group = routeBuilder
            .MapGroup("/media")
            .WithDescription("Endpoints for media operations")
            .RequireAuthorization();

        // GET - Read file content
        group.MapGet("/{mediaId:guid}/url", MediaHandler.HandleGetUrlForMediaId)
            .WithName("GetUrlForMediaId")
            .WithTags("Media")
            .WithDescription(new Description("Reads file content by filename and scope",
                "HTTP 200: Media Item returned successfully",
                "HTTP 404: Media Item not found",
                "HTTP 422: Media Id is invalid",
                "HTTP 500: Unknown error while reading media item"))
            .RequireAuthorization()
            .Produces<MediaUrlResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status500InternalServerError);

        return routeBuilder;
    }
}
