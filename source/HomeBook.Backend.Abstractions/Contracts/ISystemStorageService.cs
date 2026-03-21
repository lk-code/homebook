using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Abstractions.Contracts;

public interface ISystemStorageService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<MediaStorageSizeType>> GetMediaStorageUsageAsync(CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageUsage> GetCacheStorageUsageAsync(CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageUsage> GetLogsStorageUsageAsync(CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageUsage> GetTempDataStorageUsageAsync(CancellationToken cancellationToken);
}
