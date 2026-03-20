using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Abstractions.Contracts;

public interface ISystemStorageService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageSize> GetStorageSizeInformationsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<MediaStorageSizeType>> GetStorageSizeTypeAsync(CancellationToken cancellationToken);
}
