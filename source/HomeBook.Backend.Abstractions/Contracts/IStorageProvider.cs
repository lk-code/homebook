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
}
