using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services;

/// <inheritdoc/>
public class DeveloperService(
    IAuthenticationService authenticationService,
    BackendClient backendClient)
    : IDeveloperService
{
    private bool _isDevelopmentModeActive = false;

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // string? token = await authenticationService.GetTokenAsync(cancellationToken);

        DevModeResponse? response = await backendClient.Info.Devmode.GetAsync(x =>
            {
                // x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);
        bool isDevelopmentModeActive = response?.IsActive ?? false;
        _isDevelopmentModeActive = isDevelopmentModeActive;
    }

    /// <inheritdoc/>
    public bool IsDevelopmentModeActive() => _isDevelopmentModeActive;

    /// <inheritdoc/>
    public async Task<List<KeyValuePair<string, string?>>> GetBackendConfigurationAsync(
        CancellationToken cancellationToken =
            default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        GetDevelopmentConfigResponse? response = await backendClient.Development.Config.GetAsync(x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);
        List<KeyValuePair<string, string?>> values = response?.Values?
            .Select(x => new KeyValuePair<string, string?>(x.Key ?? string.Empty, x.Value))
            .ToList() ?? [];

        return values;
    }
}
