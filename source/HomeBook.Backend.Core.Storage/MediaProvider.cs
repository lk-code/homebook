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

        //TODO: https://homebook.com/storage/media/{mediaItemId}
        Uri mediaUri = new($"/storage/media/{mediaItemId}", UriKind.Relative);

        return mediaUri;
    }

    /// <inheritdoc/>
    public async Task<string?> GetFilenameByIdAsync(Guid mediaItemId,
        CancellationToken cancellationToken) =>
        await repository.GetFilenameByIdAsync(mediaItemId,
            cancellationToken);

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
