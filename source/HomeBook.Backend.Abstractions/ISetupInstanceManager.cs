namespace HomeBook.Backend.Abstractions;

/// <summary>
/// contract for managing the setup instance of the application.
/// </summary>
public interface ISetupInstanceManager
{
    /// <summary>
    /// Checks if a setup instance has been created.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsSetupInstanceCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a setup instance by writing the application version to a file.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateSetupInstanceAsync(CancellationToken cancellationToken = default);
}
