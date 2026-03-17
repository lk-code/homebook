using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Services;

/// <inheritdoc/>
public class FileStorageService(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : IFileStorageService
{
    /// <inheritdoc/>
    public async Task DeleteFileAsync(Guid scopeId,
        string fileName,
        CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        await backendClient.Storage.Files.DeleteAsync(x =>
            {
                x.QueryParameters.ScopeId = scopeId;
                x.QueryParameters.Filename = fileName;
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetFileAllBytesAsync(Guid scopeId,
        string fileName,
        CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        FileGetResponse? response = await backendClient.Storage.Files.GetAsFileGetResponseAsync(x =>
            {
                x.QueryParameters.ScopeId = scopeId;
                x.QueryParameters.Filename = fileName;
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        return response?.Content ?? [];
    }

    /// <inheritdoc/>
    public async Task<string> GetFileAllTextAsync(Guid scopeId,
        string fileName,
        CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<Guid> WriteFileAllBytesAsync(Guid scopeId,
        string fileName,
        byte[] content,
        CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        FilePostResponse? response = await backendClient.Storage.Files.PostAsFilePostResponseAsync(new FilePostRequest
            {
                ScopeId = scopeId,
                Filename = fileName,
                Content = content
            },
            x => x.Headers.Add("Authorization", $"Bearer {token}"),
            cancellationToken);

        return response!.MediaItemId!.Value;
    }

    /// <inheritdoc/>
    public async Task<Guid> WriteFileAllTextAsync(Guid scopeId,
        string fileName,
        string content,
        CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        throw new NotImplementedException();
    }
}
