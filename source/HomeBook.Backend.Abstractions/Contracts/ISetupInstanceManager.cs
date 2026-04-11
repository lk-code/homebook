namespace HomeBook.Backend.Abstractions.Contracts;

/// <summary>
/// contract for managing the setup instance of the application.
/// </summary>
public interface ISetupInstanceManager
{
    /// <summary>
    /// creates the required directories for the application to function properly.
    /// </summary>
    /// <returns></returns>
    void CreateRequiredDirectories();

    /// <summary>
    /// Creates a setup instance by writing the application version to a file.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateHomebookInstanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a setup instance has been created.
    /// </summary>
    /// <returns></returns>
    bool IsHomebookInstanceCreated();

    /// <summary>
    /// Checks if an update is required by comparing the current application version with the version stored in the setup instance file.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsUpdateRequiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// returns the latest installed update version.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string?> GetLatestUpdateVersionAsync(CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CopySetupFilesAsync(CancellationToken cancellationToken);
}
