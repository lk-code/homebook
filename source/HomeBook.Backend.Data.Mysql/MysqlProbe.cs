using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Setup;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace HomeBook.Backend.Data.Mysql;

/// <inheritdoc />
public class MysqlProbe(ILogger<MysqlProbe> logger) : IDatabaseProbe
{
    /// <inheritdoc />
    public string ProviderName { get; } = "MYSQL";

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
            logger.LogInformation("Checking MySQL connectivity");

            string connectionString = $"Server={host};Port={port};Database={databaseName};Uid={username};Pwd={password};Connection Timeout=5;";

            await using MySqlConnection connection = new(connectionString);
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
            logger.LogError(ex, "MySQL connectivity check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> CanConnectAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
