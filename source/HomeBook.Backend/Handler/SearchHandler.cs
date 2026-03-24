using System.Security.Claims;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Modules.Utilities;
using HomeBook.Backend.DTOs.Responses.Search;
using HomeBook.Backend.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace HomeBook.Backend.Handler;

public class SearchHandler
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="user"></param>
    /// <param name="query"></param>
    /// <param name="logger"></param>
    /// <param name="searchProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleSearch(ClaimsPrincipal user,
        [FromQuery(Name = "s")] string query,
        [FromServices] ILogger<SearchHandler> logger,
        [FromServices] ISearchProvider searchProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<ISearchAggregationResult> searchAggregationResults = await searchProvider
                .SearchAsync(query,
                    user.GetUserId(),
                    cancellationToken);

            SearchResponse response = searchAggregationResults.ToResponse();
            return TypedResults.Ok(response);
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while handling search request");
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
