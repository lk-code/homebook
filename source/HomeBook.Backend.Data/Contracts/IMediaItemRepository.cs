using HomeBook.Backend.Data.Entities;

namespace HomeBook.Backend.Data.Contracts;

/// <summary>
///
/// </summary>
public interface IMediaItemRepository
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid> AddMediaItemAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MediaItem?> GetMediaItemByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MediaItem?> GetMediaItemByFilenameAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteMediaItemAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<string?> GetFilenameByIdAsync(Guid mediaItemId, CancellationToken cancellationToken);
}
