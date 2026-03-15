using HomeBook.Backend.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Extensions;

public static class InstanceStatusExtensions
{
    /// <summary>
    /// get the current instance status based on the configuration value Database:Provider
    /// if the value is null or empty, the instance is in SETUP mode, otherwise homebook is RUNNING
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static InstanceStatus GetCurrentInstanceStatus(this IConfiguration configuration)
    {
        bool isEfCoreDesignTime = EF.IsDesignTime;
        if (isEfCoreDesignTime)
            return InstanceStatus.DEV;

        bool isGitHubWorkflow = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
        if (isGitHubWorkflow)
            return InstanceStatus.RUNNING;

        string[] commandLineArgs = Environment.GetCommandLineArgs();

        bool isHomeBookExecutable = commandLineArgs
            .First()
            .Contains("HomeBook.Backend.dll", StringComparison.OrdinalIgnoreCase);
        if (!isHomeBookExecutable)
            return InstanceStatus.DEV;

        string? databaseProvider = configuration["Database:Provider"];
        return databaseProvider switch
        {
            null or "" => InstanceStatus.SETUP,
            _ => InstanceStatus.RUNNING
        };
    }
}
