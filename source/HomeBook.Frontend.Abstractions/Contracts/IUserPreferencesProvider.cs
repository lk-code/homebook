using HomeBook.Frontend.Abstractions.Models;

namespace HomeBook.Frontend.Abstractions.Contracts;

/// <summary>
/// represents an asynchronous preference change handler.
/// </summary>
/// <typeparam name="T">the payload type of the changed preference.</typeparam>
/// <param name="value">the updated value.</param>
/// <param name="cancellationToken">the cancellation token for the handler execution.</param>
/// <returns>a task that completes when the handler finished.</returns>
public delegate Task AsyncPreferenceChangedHandler<in T>(T value, CancellationToken cancellationToken);

/// <summary>
/// defines methods to get and set user preferences
/// </summary>
public interface IUserPreferencesProvider
{
    /// <summary>
    /// event triggered when the user locale has been updated successfully.
    /// </summary>
    event AsyncPreferenceChangedHandler<string>? LocaleChanged;

    /// <summary>
    /// event triggered when the user wallpaper has been updated successfully.
    /// </summary>
    event AsyncPreferenceChangedHandler<WallpaperConfiguration>? WallpaperChanged;

    /// <summary>
    /// gets the current user locale
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string?> GetLocaleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// sets the current user locale
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetLocaleAsync(string locale, CancellationToken cancellationToken = default);

    /// <summary>
    /// gets the current user locale
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<WallpaperConfiguration?> GetWallpaperAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="wallpaper"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetStaticWallpaperAsync(string wallpaper,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="wallpaper"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetDynamicWallpaperAsync(string wallpaper,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="wallpaperMediaId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetUploadedWallpaperAsync(Guid wallpaperMediaId,
        CancellationToken cancellationToken = default);
}
