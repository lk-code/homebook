using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Search.Models;
using HomeBook.Backend.Modules.Abstractions;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Core.Search;

/// <inheritdoc/>
public class SearchProvider(
    ILogger<SearchProvider> logger,
    IEnumerable<IBackendModuleSearchRegistrar> modules) : ISearchProvider
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ISearchAggregationResult>> SearchAsync(string query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing search");

        IEnumerable<Task<SearchAggregationResult>> moduleSearchTasks = modules
            .Select(async module =>
            {
                try
                {
                    logger.LogDebug("Requesting search results from module");

                    SearchResult result = await module.SearchAsync(query,
                        userId,
                        cancellationToken);

                    logger.LogDebug("Module returned search results");

                    SearchAggregationResult moduleSearchResult = new(module.Key,
                        result.TotalCount,
                        result.Items);
                    return moduleSearchResult;
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug("Search request was cancelled");
                    return null;
                }
                catch (Exception err)
                {
                    logger.LogError(err,
                        "Error while requesting module {Module} for search query '{Query}'",
                        module.Name,
                        query);

                    return null;
                }
            })
            .Where(result => result is not null)!;

        IReadOnlyList<SearchAggregationResult> searchResults = await Task.WhenAll(moduleSearchTasks.ToArray());
        return searchResults;
    }
}
