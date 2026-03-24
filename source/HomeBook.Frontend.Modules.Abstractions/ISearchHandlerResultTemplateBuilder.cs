namespace HomeBook.Frontend.Modules.Abstractions;

public interface ISearchHandlerResultTemplateBuilder
{
    void AddSearchHandlerResultTemplate<TSearchHandlerResultTemplate>(string searchHandlerKey);
}
