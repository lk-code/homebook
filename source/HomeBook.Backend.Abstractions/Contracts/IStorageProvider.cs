namespace HomeBook.Backend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="fullScopeName"></param>
    /// <returns></returns>
    Task<bool> IsScopeRegisteredAsync(string fullScopeName,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <returns></returns>
    Task<bool> IsScopeRegisteredAsync(Guid scopeId,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="fullScopeName"></param>
    /// <param name="moduleKey"></param>
    /// <returns></returns>
    Task<Guid> RegisterStorageScopeAsync(string fullScopeName,
        string moduleKey,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <returns></returns>
    Task CreateStorageForScopeAsync(Guid scopeId,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="fullScopeName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid?> GetScopeIdByFullName(string fullScopeName,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteFileAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> GetFileAllBytesAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetFileAllTextAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid> WriteFileAllBytesAsync(Guid scopeId,
        string filename,
        byte[] content,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="scopeId"></param>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid> WriteFileAllTextAsync(Guid scopeId,
        string filename,
        string content,
        CancellationToken cancellationToken);
}
