using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.DTOs.Responses.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Handler;

public class MediaHandler
{
    // GET - Read file content
    public static async Task<IResult> HandleGetUrlForMediaId(Guid mediaId,
        [FromServices] ILogger<MediaHandler> logger,
        [FromServices] IMediaProvider mediaProvider,
        CancellationToken cancellationToken)
    {
        if (mediaId == Guid.Empty)
            return TypedResults.UnprocessableEntity("media id is empty");

        try
        {
            Uri? mediaUri = await mediaProvider.GetUrlForMediaItemAsync(mediaId,
                cancellationToken);

            if (mediaUri is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(new MediaUrlResponse(mediaUri));
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving media URL");
            return TypedResults.Problem();
        }
    }
}
