using HomeBook.Backend.Data.Entities;

namespace HomeBook.Backend.Data.Contracts;

/// <summary>
///
/// </summary>
public interface IStorageScopeRegistrationRepository
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<StorageScopeRegistration>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="fullScopeName"></param>
    /// <returns></returns>
    Task<StorageScopeRegistration?> GetByFullScopeNameAsync(string fullScopeName,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageScopeRegistration?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="fullScopeName"></param>
    /// <param name="moduleKey"></param>
    /// <returns></returns>
    Task<Guid> AddScopeAsync(string fullScopeName,
        string moduleKey,
        CancellationToken cancellationToken);
}
