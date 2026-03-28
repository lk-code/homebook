using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.Mappings;

namespace HomeBook.Frontend.Services;

public class SystemStorageProvider(
    IConfiguration configuration,
    IAuthenticationService authenticationService,
    BackendClient backendClient) : ISystemStorageProvider
{
    public async Task<StorageUsageInformations> GetStorageUsageAsync(CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        GetSystemStorageInfoResponse? response = await backendClient
            .System.Storage.Info
            .GetAsync(x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);

        return new(response?.Cache.ToDto(),
            response?.Logs.ToDto(),
            response?.Temp.ToDto(),
            (response?.MediaStorage ?? []).Select(x => new MediaStorageUsageInformation(
                x.ScopeKey,
                x.ModuleKey,
                x.ModuleName,
                (x.StorageSizeBytes ?? 0)
            ))
            .ToArray());
    }
}
