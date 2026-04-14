using HomeBook.Backend.Abstractions.Models.UserPreferences;

namespace HomeBook.Backend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IUserPreferenceProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string?> GetUserPreferredLocaleAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="locale"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetUserPreferredLocaleAsync(Guid userId, string locale, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<WallpaperConfiguration?> GetUserWallpaperAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="wallpaperConfiguration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetUserWallpaperAsync(Guid userId, string wallpaperConfiguration, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> HasUserWallpaperAsync(Guid userId, CancellationToken cancellationToken);
}
