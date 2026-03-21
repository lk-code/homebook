using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class ConfigurationRepository(
    IDbContextFactory<AppDbContext> factory,
    ILogger<ConfigurationRepository> logger) : IConfigurationRepository
{
    /// <inheritdoc />
    public async Task WriteConfigurationAsync(Configuration configuration,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Writing configuration value");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        Configuration? existingConfiguration = await GetConfigurationByKeyAsync(configuration.Key,
            cancellationToken,
            dbContext);

        if (existingConfiguration is null)
        {
            dbContext.Add(configuration);
        }
        else
        {
            existingConfiguration.Value = configuration.Value;
            dbContext.Update(existingConfiguration);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Configuration>> GetAllConfigurationAsync(CancellationToken cancellationToken =
        default)
    {
        logger.LogInformation("Retrieving all configuration values");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        List<Configuration> configurations = await dbContext.Set<Configuration>()
            .ToListAsync(cancellationToken);

        return configurations;
    }

    /// <inheritdoc />
    public async Task<Configuration?> GetConfigurationByKeyAsync(string key,
        CancellationToken cancellationToken = default,
        AppDbContext? appDbContext = null)
    {
        logger.LogDebug("Retrieving configuration value");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        Configuration? configuration = await dbContext.Set<Configuration>()
            .FirstOrDefaultAsync(c => c.Key == key, cancellationToken);

        return configuration;
    }
}
