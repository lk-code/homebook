using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Modules.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Services;

/// <inheritdoc />
public class SystemStorageService(
    IServiceProvider serviceProvider,
    IStorageScopeRegistrationRepository storageScopeRegistrationRepository,
    IApplicationPathProvider applicationPathProvider,
    IFileSystemService fileSystemService,
    ILogger<SystemStorageService> logger)
    : ISystemStorageService
{
    /// <inheritdoc />
    public async Task<List<MediaStorageSizeType>> GetMediaStorageUsageAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving storage usage grouped by scope");

        List<StorageScopeRegistration> entities = await storageScopeRegistrationRepository
            .GetAllAsync(cancellationToken);

        List<IModule> registeredModules = serviceProvider.GetServices<IModule>()
            .ToList();

        List<MediaStorageSizeType> result = new();
        foreach (StorageScopeRegistration entity in entities)
        {
            Guid scopeId = entity.Id;
            string scopeKey = entity.Name;
            IModule? scopeModule = registeredModules.FirstOrDefault(x => x.Key == entity.ModuleKey);

            // get scope module informations
            string moduleKey = "unknownmodule"; // if the module is uninstalled or removed then use this fallback
            string moduleName = "Unknown Module";
            if (scopeModule is not null)
            {
                moduleKey = scopeModule.Key; // if the module is found, then use the key and name
                moduleName = scopeModule.Name;
            }

            // get all files in storage
            string storagePath = Path.Combine(applicationPathProvider.StorageDirectory, scopeId.ToString());
            long storageSizeBytes = await GetStorageSizeByPathAsync(storagePath,
                cancellationToken);

            MediaStorageSizeType storageSizeType = new(scopeKey,
                moduleKey,
                moduleName,
                storageSizeBytes);

            result.Add(storageSizeType);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<StorageUsage> GetCacheStorageUsageAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving storage usage for cache directory");
        string path = applicationPathProvider.CacheDirectory;
        long storageSizeBytes = await GetStorageSizeByPathAsync(path,
            cancellationToken);

        return new StorageUsage("cache", storageSizeBytes);
    }

    /// <inheritdoc />
    public async Task<StorageUsage> GetLogsStorageUsageAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving storage usage for logs directory");
        string path = applicationPathProvider.LogDirectory;
        long storageSizeBytes = await GetStorageSizeByPathAsync(path,
            cancellationToken);

        return new StorageUsage("logs", storageSizeBytes);
    }

    /// <inheritdoc />
    public async Task<StorageUsage> GetTempDataStorageUsageAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving storage usage for temp data directory");
        string path = applicationPathProvider.TempDirectory;
        long storageSizeBytes = await GetStorageSizeByPathAsync(path,
            cancellationToken);

        return new StorageUsage("temp", storageSizeBytes);
    }

    private async Task<long> GetStorageSizeByPathAsync(string storagePath,
        CancellationToken cancellationToken)
    {
        List<FileInformation> files = await fileSystemService.GetFilesInDirectoryAsync(storagePath,
            cancellationToken);
        long storageSizeBytes = files.Sum(x => x.SizeBytes);
        return storageSizeBytes;
    }
}
