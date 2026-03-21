using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Sqlite;

public class DatabaseManager(ILogger<DatabaseManager> logger) : IDatabaseManager
{
    /// <inheritdoc />
    public Task<bool> IsDatabaseAvailableAsync(string databaseHost,
        ushort databasePort,
        string databaseName,
        string databaseUserName,
        string databaseUserPassword,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> IsDatabaseFileAvailableAsync(string databaseFilePath, CancellationToken cancellationToken)
    {
        try
        {
            string connectionString = $"Data Source={databaseFilePath};Timeout=5;";

            logger.LogInformation("Checking SQLite database availability");

            await using SqliteConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            return true;
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while checking SQLite database availability");

            return false;
        }
    }
}
