using HomeBook.Backend.Abstractions.Contracts;

namespace HomeBook.Backend.Abstractions.Models;

public record SearchResult(
    int TotalCount,
    IEnumerable<ISearchResultItem> Items);

public record SearchResultItem(
    string Title,
    string? Description,
    string Identifier) : ISearchResultItem;
