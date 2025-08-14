namespace HomeBook.Backend.Abstractions;

/// <summary>
/// contract for a database manager which brings methods for database management tasks.
/// </summary>
public interface IDatabaseManager
{
    /// <summary>
    /// checks if the database is available with the given configuration.
    /// </summary>
    /// <param name="databaseHost"></param>
    /// <param name="databasePort"></param>
    /// <param name="databaseName"></param>
    /// <param name="databaseUserName"></param>
    /// <param name="databaseUserPassword"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsDatabaseAvailableAsync(string databaseHost,
        ushort databasePort,
        string databaseName,
        string databaseUserName,
        string databaseUserPassword,
        CancellationToken cancellationToken);
}
