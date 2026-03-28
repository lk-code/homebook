using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Setup;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data;

/// <inheritdoc />
public class DatabaseProviderResolver(
    IEnumerable<IDatabaseProbe> databaseProbes,
    ILogger<DatabaseProviderResolver> logger) : IDatabaseProviderResolver
{
    /// <inheritdoc />
    public async Task<string?> ResolveAsync(string host,
        ushort port,
        string databaseName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Resolving database provider");

        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Run all database probes in parallel
        IEnumerable<Task<string?>> probeTasks = databaseProbes.Select(async probe =>
        {
            try
            {
                logger.LogDebug("Running database probe");

                bool canConnect = await probe.CanConnectAsync(host, port, databaseName, username, password, cts.Token);
                return canConnect ? probe.ProviderName : (string?)null;
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Database probe was cancelled");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database probe failed");
                return null;
            }
        });

        Task<string?>[] taskArray = probeTasks.ToArray();

        // Wait for the first successful result
        while (taskArray.Length > 0)
        {
            Task<string?> completedTask = await Task.WhenAny(taskArray);
            string? result = await completedTask;

            if (!string.IsNullOrEmpty(result))
            {
                logger.LogInformation("Resolved database provider");
                await cts.CancelAsync();
                return result;
            }

            // Remove the completed task and continue with remaining tasks
            taskArray = taskArray.Where(t => t != completedTask).ToArray();
        }

        logger.LogWarning("No database provider could be resolved");
        return null;
    }
}
