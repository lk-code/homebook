using HomeBook.Backend.Abstractions.Contracts;
using Homebook.Backend.Core.Setup.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup;

public class UpdateProcessor(
    ILogger<UpdateProcessor> logger,
    IConfiguration configuration,
    ISetupInstanceManager setupInstanceManager,
    IUpdateManager updateManager,
    IDatabaseMigratorFactory databaseMigratorFactory) : IUpdateProcessor
{
    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        string? databaseType = configuration["Database:Provider"];
        if (string.IsNullOrEmpty(databaseType))
            throw new SetupException("database provider is not configured");

        try
        {
            logger.LogInformation("Starting update process...");

            // 1. create directory structure ( to create new directories from this update)
            setupInstanceManager.CreateRequiredDirectories();

            // 2. execute database migrations
            logger.LogInformation("Starting database migration for provider: {DatabaseType}", databaseType);
            IDatabaseMigrator databaseMigrator = databaseMigratorFactory.CreateMigrator(databaseType);
            await databaseMigrator.MigrateAsync(cancellationToken);

            // 3. copy setup files
            await setupInstanceManager.CopySetupFilesAsync(cancellationToken);

            // 4. execute all available updates
            await updateManager.ExecuteAvailableUpdateAsync(cancellationToken);

            // 5. write current version to file
            await setupInstanceManager.CreateHomebookInstanceAsync(cancellationToken);

            logger.LogInformation("Finish update process...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during update process, update aborted");
            throw new SetupException("Error during update process, update aborted");
        }
    }
}
