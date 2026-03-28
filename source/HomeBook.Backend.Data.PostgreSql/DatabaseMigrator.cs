using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.PostgreSql;

/// <inheritdoc />
public class DatabaseMigrator(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IEnumerable<SaveChangesInterceptor> saveChangesInterceptors,
    ILogger<DatabaseMigrator> logger) : IDatabaseMigrator
{
    /// <inheritdoc />
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Running PostgreSQL database migrations");

        await using AppDbContext context = (AppDbContext)GetDbContext();
        await context.Database.MigrateAsync(cancellationToken);
    }

    /// <inheritdoc />
    public DbContext GetDbContext()
    {
        logger.LogDebug("Creating PostgreSQL database context for migration");

        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        ServiceCollectionExtensions.CreateDbContextOptionsBuilder(configuration, serviceProvider, optionsBuilder);

        AppDbContext context = new(optionsBuilder.Options,
            saveChangesInterceptors);
        return context;
    }

    public void ConfigureForServiceCollection(ServiceCollection services, IConfiguration configuration)
    {
        logger.LogInformation("Configuring PostgreSQL services for migration");
        services.AddBackendDataPostgreSql(configuration);
    }
}
