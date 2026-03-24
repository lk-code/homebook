namespace HomeBook.Backend.DTOs.Responses.Search;

public record SearchResponse(SearchModuleResponse[] SearchModuleResponses);

public record SearchModuleResponse(
    string ModuleKey,
    int TotalCount,
    IEnumerable<SearchItemResponse> Items);

public record SearchItemResponse(
    string Title,
    string? Description,
    string Identifier);
