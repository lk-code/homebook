namespace HomeBook.Backend.Abstractions.Contracts;

public interface ISearchResultItem
{
    string Title { get; }
    string? Description { get; }
    string Identifier { get; }
}
