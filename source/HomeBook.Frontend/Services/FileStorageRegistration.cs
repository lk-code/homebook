using HomeBook.Client;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Services;

/// <inheritdoc/>
public class FileStorageRegistration(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : IFileStorageRegistration
{
    /// <inheritdoc/>
    public async Task<Guid?> GetScopeIdForModuleAsync(IModule module,
        string scopeName,
        CancellationToken cancellationToken = default)
    {
        string fullScopeName = $"{module.Key}.{scopeName}";

        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        Guid? scopeId = await backendClient.Storage.Scopes
            .GetAsync(x =>
            {
                x.QueryParameters.Name = fullScopeName;
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        return scopeId;
    }
}
