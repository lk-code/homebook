using HomeBook.Client;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Contracts;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Services;

/// <inheritdoc/>
public class FileStorageRegistration(
    IAuthenticationService authenticationService,
    BackendClient backendClient)
    : IFileStorageRegistration,
        IInternalFileStorageRegistration
{
    /// <summary>
    /// returns all allowed file extensions for images
    /// </summary>
    public string FileExtForImages { get; } = ".png, .jpg, .jpeg, .webp";

    /// <inheritdoc/>
    public async Task<Guid?> GetScopeIdForModuleAsync(IModule module,
        string scopeName,
        CancellationToken cancellationToken = default)
    {
        string fullScopeName = $"{module.Key}.{scopeName}";

        return await GetScopeAsync(cancellationToken, fullScopeName);
    }

    /// <inheritdoc/>
    public async Task<Guid?> GetScopeIdForModuleAsync(string moduleKey,
        string scopeName,
        CancellationToken cancellationToken = default)
    {
        string fullScopeName = $"{moduleKey}.{scopeName}";

        return await GetScopeAsync(cancellationToken, fullScopeName);
    }

    private async Task<Guid?> GetScopeAsync(CancellationToken cancellationToken, string fullScopeName)
    {
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
