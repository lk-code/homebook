using HomeBook.Backend.Data.Entities;

namespace HomeBook.Backend.Data.Contracts;

/// <summary>
/// defines methods to interact with user preferences in the data store
/// </summary>
public interface IUserPreferenceRepository
{
    /// <summary>
    /// gets the preference value for a specific user by key
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserPreference?> GetPreferenceForUserByKeyAsync(Guid userId,
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    /// sets the preference value for a specific user by key
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetPreferenceAsync(UserPreference entity,
        CancellationToken cancellationToken);
}
