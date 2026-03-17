using System.Globalization;
using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Enums;
using HomeBook.Frontend.UI.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HomeBook.Frontend.Services.Services;

/// <inheritdoc />
public class LocalizationService(
    IAuthenticationService authenticationService,
    IInstanceManagementProvider instanceManagementProvider,
    NavigationManager navigationManager,
    IUserPreferencesProvider userPreferencesProvider,
    IJSRuntime jsRuntime) : ILocalizationService, IAsyncDisposable
{
    private string _defaultLocale = null!;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string defaultLocale = await instanceManagementProvider.GetDefaultLocaleAsync(cancellationToken);
        _defaultLocale = defaultLocale;

        authenticationService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        authenticationService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    /// <inheritdoc />
    public async Task<string> GetCultureAsync(CancellationToken cancellationToken = default)
    {
        string culture = await jsRuntime.InvokeAsync<string>("localStorage.getItem",
            cancellationToken,
            JsLocalStorageKeys.UserPreferenceLocale);

        if (!string.IsNullOrEmpty(culture))
            return culture;

        return _defaultLocale;
    }

    /// <inheritdoc />
    public async Task SetCultureAsync(string selectedCulture,
        bool forceLoad = true,
        CancellationToken cancellationToken = default)
    {
        // UI + Formatkultur → vom Benutzer gewählt
        var uiCulture = new CultureInfo(selectedCulture);

        // Ressourcen-Kultur → gemappt
        var resourceCulture = LocalizationCultureMapper.GetResourceCulture(selectedCulture);

        CultureInfo.DefaultThreadCurrentCulture = uiCulture;
        CultureInfo.DefaultThreadCurrentUICulture = resourceCulture;

        await jsRuntime.InvokeVoidAsync("localStorage.setItem",
            cancellationToken,
            JsLocalStorageKeys.UserPreferenceLocale,
            selectedCulture);

        if (forceLoad)
            navigationManager.NavigateTo(navigationManager.Uri, forceLoad: true);
    }

    private void OnAuthenticationStateChanged(bool isAuthenticated)
    {
        if (!isAuthenticated)
            Task.Run(() => SetCultureAsync(_defaultLocale, true));
        else
            Task.Run(() => LoadCultureForUserAsync(true));
    }

    private async Task LoadCultureForUserAsync(bool forceReload = true)
    {
        CancellationToken cancellationToken = CancellationToken.None;

        if (!await authenticationService.IsAuthenticatedAsync(cancellationToken))
            return;

        string? locale = await userPreferencesProvider.GetLocaleAsync(cancellationToken);

        if (string.IsNullOrEmpty(locale))
            locale = _defaultLocale;

        await SetCultureAsync(locale!,
            forceReload,
            cancellationToken);
    }
}
