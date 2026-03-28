namespace HomeBook.Frontend.Modules.Abstractions;

/// <summary>
///
/// </summary>
public interface IMediaService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="mediaItemId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Uri?> GetUrlForMediaItemAsync(Guid mediaItemId,
        CancellationToken cancellationToken);
}
