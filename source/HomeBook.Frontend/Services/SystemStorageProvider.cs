using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;

namespace HomeBook.Frontend.Services;

public class SystemStorageProvider(
    IConfiguration configuration,
    IAuthenticationService authenticationService,
    BackendClient backendClient) : ISystemStorageProvider
{
    public async Task<StorageSize> GetStorageUsageAsync(CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        GetSystemStorageInfoResponse? response = await backendClient
            .System.Storage.Info
            .GetAsync(x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);

        return new(response?.Total ?? 0,
            response?.Used ?? 0,
            response?.Free ?? 0,
            (response?.StorageByType ?? []).Select(x => new MediaStorageSizeType(
                x.ScopeKey,
                x.ModuleKey,
                x.ModuleName,
                (x.StorageSizeBytes ?? 0)
            ))
            .ToArray());
    }
}
