using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HomeBook.Backend.Data.PostgreSql;

/// <inheritdoc />
public class PostgreSqlProbe(ILogger<PostgreSqlProbe> logger) : IDatabaseProbe
{
    /// <inheritdoc />
    public string ProviderName { get; } = "POSTGRESQL";

    /// <inheritdoc />
    public async Task<bool> CanConnectAsync(string host,
        ushort port,
        string databaseName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Checking PostgreSQL connectivity");

            string connectionString = $"Host={host};Port={port};Database={databaseName};Username={username};Password={password};Timeout=5;";

            await using NpgsqlConnection connection = new(connectionString);
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
            logger.LogError(ex, "PostgreSQL connectivity check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> CanConnectAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
