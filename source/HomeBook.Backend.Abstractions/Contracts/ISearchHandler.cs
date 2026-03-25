using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Abstractions.Contracts;

/// <summary>
///
/// </summary>
public interface ISearchHandler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SearchResult> SearchAsync(string query,
        Guid userId,
        CancellationToken cancellationToken = default);
}
