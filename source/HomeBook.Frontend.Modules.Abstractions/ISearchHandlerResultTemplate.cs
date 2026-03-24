namespace HomeBook.Frontend.Modules.Abstractions;

public interface ISearchHandlerResultTemplate
{
    IReadOnlyList<SearchHandlerResultTemplateItem> Items { get; set; }
}
