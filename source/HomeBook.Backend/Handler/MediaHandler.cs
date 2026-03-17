using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.DTOs.Responses.Media;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public static class MediaHandler
{
    // GET - Read file content
    public static async Task<IResult> HandleGetUrlForMediaId(Guid mediaId,
        [FromServices] IMediaProvider mediaProvider,
        CancellationToken cancellationToken)
    {
        if (mediaId == Guid.Empty)
            return TypedResults.UnprocessableEntity("media id is empty");

        try
        {
            Uri? mediaUri = await mediaProvider.GetUrlForMediaItemAsync(mediaId,
                cancellationToken);

            if(mediaUri is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(new MediaUrlResponse(mediaUri));
        }
        catch (Exception err)
        {
            // TODO: log and return error
            return TypedResults.Problem();
        }
    }
}
