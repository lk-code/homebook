namespace HomeBook.Frontend.Modules.Abstractions;

/// <summary>
///
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteFileAsync(Guid scopeId, string fileName, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> GetFileAllBytesAsync(Guid scopeId, string fileName, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetFileAllTextAsync(Guid scopeId, string fileName, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="fileName"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteFileAllBytesAsync(Guid scopeId, string fileName, byte[] content, CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="fileName"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteFileAllTextAsync(Guid scopeId, string fileName, string content, CancellationToken cancellationToken);
}
