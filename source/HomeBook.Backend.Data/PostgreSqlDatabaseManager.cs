using HomeBook.Backend.Abstractions;
using Npgsql;

namespace HomeBook.Backend.Data;

/// <inheritdoc />
public class PostgreSqlDatabaseManager : IDatabaseManager
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
            string connectionString = $"Host={databaseHost};Port={databasePort};Database={databaseName};Username={databaseUserName};Password={databaseUserPassword};Timeout=5;";

            await using NpgsqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
