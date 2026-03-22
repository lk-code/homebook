namespace HomeBook.Backend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface IDeveloperProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsDevelopmentModeActiveAsync(CancellationToken cancellationToken);
}
