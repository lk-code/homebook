namespace HomeBook.Frontend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IDeveloperService
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    bool IsDevelopmentModeActive();

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<KeyValuePair<string, string?>>> GetBackendConfigurationAsync(CancellationToken cancellationToken =
        default);
}
