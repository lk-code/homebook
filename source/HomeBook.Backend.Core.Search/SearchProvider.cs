using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Core.Search.Models;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Core.Search;

/// <inheritdoc/>
public class SearchProvider(
    ILogger<SearchProvider> logger,
    IEnumerable<ISearchHandler> searchHandlers,
    IEnumerable<SearchHandlerRegistration> searchHandlerRegistrations) : ISearchProvider
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ISearchAggregationResult>> SearchAsync(string query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing search");

        IEnumerable<Task<SearchAggregationResult>> moduleSearchTasks = searchHandlers
            .Select(async searchHandler =>
            {
                try
                {
                    var type = searchHandler.GetType();
                    string moduleKey = searchHandlerRegistrations.First(x => x.SearchHandlerType == type).ModuleId;
                    string searchModuleKey = $"{moduleKey}.{type.Name}";

                    logger.LogInformation("Handling search request for search handler '{0}'", searchModuleKey);

                    SearchResult searchResult = await searchHandler
                        .SearchAsync(query, userId, cancellationToken);

                    return new SearchAggregationResult(searchModuleKey,
                        searchResult.TotalCount,
                        searchResult.Items);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Search request was cancelled");
                    return null;
                }
                catch (Exception err)
                {
                    // logger.LogError(err,
                    //     "Error while requesting module {Module} for search query '{Query}'",
                    //     module.Name,
                    //     query);

                    return null;
                }
            })
            .Where(result => result is not null)!;

        IReadOnlyList<SearchAggregationResult> searchResults = await Task.WhenAll(moduleSearchTasks.ToArray());
        return searchResults;
    }
}
