using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Module.Kitchen.Contracts;
using HomeBook.Backend.Module.Kitchen.Models;

namespace HomeBook.Backend.Module.Kitchen.SearchHandler;

/// <inheritdoc/>
public class RecipeSearchHandler(IRecipesProvider recipesProvider) : ISearchHandler
{
    /// <inheritdoc/>
    public async Task<SearchResult> SearchAsync(string query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        RecipeResultDto[] recipes = await recipesProvider.GetRecipesAsync(query,
            cancellationToken);

        List<ISearchResultItem> items = recipes.Select(recipe => new SearchResultItem(recipe.Name,
                recipe.Description,
                recipe.Id.ToString()))
            .Cast<ISearchResultItem>()
            .ToList();

        return new SearchResult(items.Count,
            items);
    }
}
