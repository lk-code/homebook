using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.DTOs.Responses.Search;

namespace HomeBook.Backend.Mappings;

public static class SearchMappings
{
    public static SearchResponse ToResponse(this IEnumerable<ISearchAggregationResult> results)
    {
        List<SearchModuleResponse> moduleResponses = [];
        moduleResponses.AddRange(results
            .Select(result => new SearchModuleResponse(
                result.ModuleKey,
                result.TotalCount,
                result.Items.Select(x => x.ToResponse()))));

        SearchResponse response = new(moduleResponses.ToArray());
        return response;
    }

    public static SearchItemResponse ToResponse(this ISearchResultItem item) =>
        new(
            item.Title,
            item.Description,
            item.Identifier);
}
