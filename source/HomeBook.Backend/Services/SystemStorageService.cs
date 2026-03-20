using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Services;

/// <inheritdoc />
public class SystemStorageService : ISystemStorageService
{
    /// <inheritdoc />
    public async Task<StorageSize> GetStorageSizeInformationsAsync(CancellationToken cancellationToken)
    {
        var drive = DriveInfo.GetDrives()
            .First(d => d.IsReady
                        && d.Name == Path.GetPathRoot(Environment.SystemDirectory));

        long totalBytes = drive.TotalSize;
        long freeBytes = drive.AvailableFreeSpace;
        long usedBytes = totalBytes - freeBytes;

        StorageSize  storageSize = new(totalBytes, usedBytes, freeBytes);
        return storageSize;
    }
}
