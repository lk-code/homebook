using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Sqlite;

public class SqliteProbe(ILogger<SqliteProbe> logger) : IDatabaseProbe
{
    /// <inheritdoc />
    public string ProviderName { get; } = "SQLITE";

    /// <inheritdoc />
    public Task<bool> CanConnectAsync(string host,
        ushort port,
        string databaseName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> CanConnectAsync(string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Checking SQLite connectivity");

            string connectionString = ConnectionStringBuilder.Build(filePath);

            await using SqliteConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            bool isConnected = connection.State == System.Data.ConnectionState.Open;
            if (isConnected)
            {
                await connection.CloseAsync();
            }

            return isConnected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SQLite connectivity check failed");
            return false;
        }
    }
}
