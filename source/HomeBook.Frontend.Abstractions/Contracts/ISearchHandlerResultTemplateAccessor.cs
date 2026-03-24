namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISearchHandlerResultTemplateAccessor
{
    Dictionary<string, Type> GetSearchHandlerResultTemplates();
}
