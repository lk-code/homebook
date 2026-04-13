using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IWallpaperProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="wallpaper"></param>
    /// <returns></returns>
    string GetAbsolutFilePathForWallpaper(string wallpaper);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<SystemWallpaperDto>> GetSystemWallpapersAsync(CancellationToken cancellationToken =
        default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<MediaItemWallpaperDto>> GetUploadedWallpapersAsync(CancellationToken cancellationToken =
        default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="wallpaperFile"></param>
    /// <returns></returns>
    Task<Dictionary<string, List<string>>?> GetWallpaperConfigurationAsync(string wallpaperFile,
        CancellationToken cancellationToken = default);
}
