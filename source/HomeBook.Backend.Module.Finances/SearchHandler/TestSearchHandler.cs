using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Module.Finances.SearchHandler;

public class TestSearchHandler : ISearchHandler
{
    public async Task<SearchResult> SearchAsync(string query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return new SearchResult(0,
            []);
    }
}
