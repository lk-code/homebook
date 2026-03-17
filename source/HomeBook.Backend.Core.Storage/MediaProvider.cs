using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.Media;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;

namespace HomeBook.Backend.Core.Storage;

/// <inheritdoc/>
public class MediaProvider(IMediaItemRepository repository) : IMediaProvider
{
    /// <inheritdoc/>
    public async Task<Uri?> GetUrlForMediaItemAsync(Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        MediaItem? mediaItem = await repository.GetMediaItemByIdAsync(mediaItemId,
            cancellationToken);

        if (mediaItem is null)
            return null;

        //TODO: https://homebook.com/storage/file/{mediaItemId}
        Uri mediaUri = new($"/storage/file/{mediaItemId}", UriKind.Relative);

        return mediaUri;
    }

    /// <inheritdoc/>
    public async Task<MediaItemDto?> GetMediaItemByIdAsync(Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        MediaItem? mediaItem = await repository.GetMediaItemByIdAsync(mediaItemId,
            cancellationToken);

        if (mediaItem is null)
            return null;

        MediaItemDto mediaItemDto = new(mediaItem.Id,
            mediaItem.StorageScopeId,
            mediaItem.FileName);

        return mediaItemDto;
    }
}
