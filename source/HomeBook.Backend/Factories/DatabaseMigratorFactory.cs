using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Exceptions;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Factories;

/// <inheritdoc />
public class DatabaseMigratorFactory(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigratorFactory> logger)
    : IDatabaseMigratorFactory
{
    /// <inheritdoc />
    public IDatabaseMigrator CreateMigrator(string databaseType)
    {
        string key = databaseType.ToUpperInvariant();
        logger.LogInformation("Creating database migrator for provider {DatabaseType}", key);

        IDatabaseMigrator migrator = serviceProvider.GetKeyedService<IDatabaseMigrator>(key)
                                     ?? throw new UnsupportedDatabaseException(
                                         $"Unsupported database provider: {databaseType}");

        return migrator;
    }
}
