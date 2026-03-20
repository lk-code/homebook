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
}
