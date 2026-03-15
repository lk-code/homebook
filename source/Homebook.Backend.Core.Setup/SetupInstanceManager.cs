using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup;

/// <inheritdoc />
public class SetupInstanceManager(
    ILogger<SetupInstanceManager> logger,
    IConfiguration configuration,
    IFileSystemService fileSystemService,
    IApplicationPathProvider applicationPathProvider) : ISetupInstanceManager
{
    private string _homebookInstanceFileName => Path.Combine(applicationPathProvider.DataDirectory, ".homebook");

    /// <inheritdoc />
    public void CreateRequiredDirectories()
    {
        string[] requiredDirectories =
        [
            applicationPathProvider.ConfigurationPath,
            applicationPathProvider.CacheDirectory,
            applicationPathProvider.LogDirectory,
            applicationPathProvider.DataDirectory,
            applicationPathProvider.TempDirectory,
            applicationPathProvider.UpdateDirectory,
            applicationPathProvider.StorageDirectory
        ];

        foreach (string dir in requiredDirectories)
        {
            if (fileSystemService.DirectoryExists(dir))
                continue;

            logger.LogInformation("Creating required directory at {Directory}", dir);
            fileSystemService.CreateDirectory(dir);
        }
    }

    /// <inheritdoc />
    public async Task CreateHomebookInstanceAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Write homebook instance file at {FilePath}", _homebookInstanceFileName);

        string appVersion = configuration.GetSection("Version")?.Value?.Trim() ?? string.Empty;
        await fileSystemService.FileWriteAllTextAsync(_homebookInstanceFileName,
            appVersion,
            cancellationToken);
    }

    /// <inheritdoc />
    public bool IsHomebookInstanceCreated()
    {
        logger.LogInformation("Checking if homebook instance file exists at {FilePath}", _homebookInstanceFileName);

        return
            fileSystemService.FileExists(
                _homebookInstanceFileName); // true => means setup is already executed and instance is created
    }

    /// <inheritdoc />
    public async Task<bool> IsUpdateRequiredAsync(CancellationToken cancellationToken = default)
    {
        // get the version from the appsettings
        string? runningAppVersion = configuration.GetSection("Version")?.Value?.Trim();
        string? installedInstanceVersion = null;
        try
        {
            // get the version from the instance file
            if (fileSystemService.FileExists(_homebookInstanceFileName))
                installedInstanceVersion = await fileSystemService.FileReadAllTextAsync(_homebookInstanceFileName,
                    cancellationToken);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error reading instance version from file");
            return false;
        }

        if (string.IsNullOrEmpty(runningAppVersion)
            || string.IsNullOrEmpty(installedInstanceVersion))
            return false;

        int versionComparison = new Version(runningAppVersion).CompareTo(new Version(installedInstanceVersion));
        if (versionComparison <= 0)
            return false;

        return true;
    }

    /// <inheritdoc />
    public async Task<string?> GetLatestUpdateVersionAsync(CancellationToken cancellationToken)
    {
        bool isHomebookInstanceCreated = IsHomebookInstanceCreated();
        if (!isHomebookInstanceCreated)
            return null;

        string installedInstanceVersion = await fileSystemService.FileReadAllTextAsync(_homebookInstanceFileName,
            cancellationToken);

        return installedInstanceVersion;
    }
}
