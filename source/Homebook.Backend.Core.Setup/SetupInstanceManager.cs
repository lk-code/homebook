using HomeBook.Backend.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup;

/// <inheritdoc />
public class SetupInstanceManager(
    ILogger<SetupInstanceManager> logger,
    IConfiguration configuration,
    IFileService fileService) : ISetupInstanceManager
{
    private const string INSTANCE_FILE_PATH = "/var/lib/homebook/instance.txt";

    /// <inheritdoc />
    public async Task<bool> IsSetupInstanceCreatedAsync(CancellationToken cancellationToken = default)
    {
        bool instanceFileExists = await fileService.DoesFileExistsAsync(INSTANCE_FILE_PATH);

        return instanceFileExists; // true => means setup is already executed and instance is created
    }

    /// <inheritdoc />
    public async Task CreateSetupInstanceAsync(CancellationToken cancellationToken = default)
    {
        string appVersion = configuration["Version"] ?? string.Empty;
        await File.WriteAllTextAsync(INSTANCE_FILE_PATH, appVersion, cancellationToken);
    }
}
