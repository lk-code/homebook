using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Modules.Abstractions;

namespace HomeBook.Backend.Core.Search;

/// <inheritdoc/>
public class SearchBuilder() :
    ISearchBuilder,
    ISearchBuilderDataAccessor
{
    private List<Type> handlers = [];

    /// <inheritdoc/>
    public void RegisterHandler<ISearchHandler>()
    {
        handlers.Add(typeof(ISearchHandler));
    }

    /// <inheritdoc/>
    public IEnumerable<Type> GetRegisteredSearchHandlers() => handlers;
}
