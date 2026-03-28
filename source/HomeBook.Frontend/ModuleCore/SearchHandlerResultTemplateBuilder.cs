using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.ModuleCore;

/// <inheritdoc/>
public class SearchHandlerResultTemplateBuilder : ISearchHandlerResultTemplateBuilder,
    ISearchHandlerResultTemplateAccessor
{
    private readonly Dictionary<string, Type> _searchHandlerResultTemplates = [];

    /// <inheritdoc/>
    public void AddSearchHandlerResultTemplate<TSearchHandlerResultTemplate>(string searchHandlerKey)
    {
        _searchHandlerResultTemplates[searchHandlerKey] = typeof(TSearchHandlerResultTemplate);
    }

    /// <inheritdoc/>
    public Dictionary<string, Type> GetSearchHandlerResultTemplates() => _searchHandlerResultTemplates;
}
