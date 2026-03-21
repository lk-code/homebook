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
    public async Task<StorageSize> GetStorageSizeInformationsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving system storage information");

        DriveInfo drive = DriveInfo.GetDrives()
            .First(d => d.IsReady
                        && d.Name == Path.GetPathRoot(Environment.SystemDirectory));

        long totalBytes = drive.TotalSize;
        long freeBytes = drive.AvailableFreeSpace;
        long usedBytes = (totalBytes - freeBytes);

        StorageSize storageSize = new(totalBytes,
            usedBytes,
            freeBytes);
        return storageSize;
    }

    /// <inheritdoc />
    public async Task<List<MediaStorageSizeType>> GetStorageSizeTypeAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving storage usage grouped by scope");

        List<StorageScopeRegistration> entities =
            await storageScopeRegistrationRepository.GetAllAsync(cancellationToken);

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
            List<FileInformation> files = await fileSystemService.GetFilesInDirectoryAsync(storagePath,
                cancellationToken);
            long storageSizeBytes = files.Sum(x => x.SizeBytes);

            MediaStorageSizeType storageSizeType = new(scopeKey,
                moduleKey,
                moduleName,
                storageSizeBytes);

            result.Add(storageSizeType);
        }

        return result;
    }
}
