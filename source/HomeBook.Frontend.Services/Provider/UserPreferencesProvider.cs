using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;

namespace HomeBook.Frontend.Services.Provider;

/// <inheritdoc />
public class UserPreferencesProvider(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : IUserPreferencesProvider
{
    /// <inheritdoc />
    public event AsyncPreferenceChangedHandler<string>? LocaleChanged;

    /// <inheritdoc />
    public event AsyncPreferenceChangedHandler<WallpaperConfiguration>? WallpaperChanged;

    /// <inheritdoc />
    public async Task<string?> GetLocaleAsync(CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        GetUserPreferenceLocaleResponse? response = await backendClient.User.Preferences.Locale.GetAsync(x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        return response?.Locale;
    }

    /// <inheritdoc />
    public async Task SetLocaleAsync(string locale,
        CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        await backendClient.User.Preferences.Locale.PostAsync(new UpdateUserPreferenceLocaleRequest()
            {
                Locale = locale
            },
            x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        await InvokeAsync(LocaleChanged, locale, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WallpaperConfiguration?> GetWallpaperAsync(CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        GetUserPreferenceWallpaperResponse? response = await backendClient.User.Preferences.Wallpaper.GetAsync(x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        if (string.IsNullOrEmpty(response?.Wallpaper))
            return null;

        WallpaperConfiguration conf = WallpaperConfiguration.Parse(response.Config);
        return conf;
    }

    /// <inheritdoc />
    public async Task SetStaticWallpaperAsync(string wallpaper,
        CancellationToken cancellationToken = default)
    {
        WallpaperConfiguration conf = new(WallpaperType.Static,
            null,
            wallpaper,
            null);

        await SetWallpaperAsync(conf, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetDynamicWallpaperAsync(string wallpaper,
        CancellationToken cancellationToken = default)
    {
        WallpaperConfiguration conf = new(WallpaperType.Dynamic,
            null,
            null,
            wallpaper);

        await SetWallpaperAsync(conf, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetUploadedWallpaperAsync(Guid wallpaperMediaId,
        CancellationToken cancellationToken = default)
    {
        WallpaperConfiguration conf = new(WallpaperType.Uploaded,
            wallpaperMediaId,
            null,
            null);

        await SetWallpaperAsync(conf, cancellationToken);
    }

    public async Task SetWallpaperAsync(WallpaperConfiguration conf,
        CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        await backendClient.User.Preferences.Wallpaper.PostAsync(new UpdateUserPreferenceWallpaperRequest()
            {
                WallpaperConfiguration = conf.ToString()
            },
            x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        await InvokeAsync(WallpaperChanged, conf, cancellationToken);
    }

    private static Task InvokeAsync<T>(AsyncPreferenceChangedHandler<T>? eventHandler,
        T value,
        CancellationToken cancellationToken)
    {
        if (eventHandler is null)
            return Task.CompletedTask;

        Delegate[] delegates = eventHandler.GetInvocationList();
        Task[] tasks = new Task[delegates.Length];

        for (int i = 0; i < delegates.Length; i++)
            tasks[i] = ((AsyncPreferenceChangedHandler<T>)delegates[i]).Invoke(value, cancellationToken);

        return Task.WhenAll(tasks);
    }
}
