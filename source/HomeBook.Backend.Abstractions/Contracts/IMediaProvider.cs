using HomeBook.Backend.Abstractions.Models.Media;

namespace HomeBook.Backend.Abstractions.Contracts;

public interface IMediaProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="mediaItemId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri?> GetUrlForMediaItemAsync(Guid mediaItemId,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="mediaItemId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MediaItemDto?> GetMediaItemByIdAsync(Guid mediaItemId,
        CancellationToken cancellationToken);
}
