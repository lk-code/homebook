using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace HomeBook.Backend.Data.Mysql;

/// <inheritdoc />
public class DatabaseManager(ILogger<DatabaseManager> logger) : IDatabaseManager
{
    /// <inheritdoc />
    public async Task<bool> IsDatabaseAvailableAsync(string databaseHost,
        ushort databasePort,
        string databaseName,
        string databaseUserName,
        string databaseUserPassword,
        CancellationToken cancellationToken)
    {
        try
        {
            string connectionString =
                $"Host={databaseHost};Port={databasePort};Database={databaseName};Username={databaseUserName};Password={databaseUserPassword};Timeout=5;";

            logger.LogInformation("Checking MySQL database availability");

            await using MySqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            return true;
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while checking MySQL database availability");

            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> IsDatabaseFileAvailableAsync(string databaseFilePath, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
