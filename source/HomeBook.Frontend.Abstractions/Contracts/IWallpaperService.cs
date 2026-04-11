using HomeBook.Frontend.Abstractions.Models;

namespace HomeBook.Frontend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IWallpaperService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<WallpaperDto>> GetAllWallpapersAsync(CancellationToken cancellationToken);
}
