using HomeBook.Frontend.Module.Kitchen.Models;

namespace HomeBook.Frontend.Module.Kitchen.Contracts;

public interface IRecipeService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<RecipeDto>> GetRecipesAsync(string? filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RecipeDetailDto?> GetRecipeByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="servings"></param>
    /// <param name="steps"></param>
    /// <param name="ingredients"></param>
    /// <param name="durationWorkingMinutes"></param>
    /// <param name="durationCookingMinutes"></param>
    /// <param name="durationRestingMinutes"></param>
    /// <param name="caloriesKcal"></param>
    /// <param name="comments"></param>
    /// <param name="source"></param>
    /// <param name="mediaItems"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateOrUpdateRecipeAsync(Guid? id,
        string name,
        string? description = null,
        int? servings = null,
        RecipeStepDto[]? steps = null,
        RecipeIngredientDto[]? ingredients = null,
        int? durationWorkingMinutes = null,
        int? durationCookingMinutes = null,
        int? durationRestingMinutes = null,
        int? caloriesKcal = null,
        string? comments = null,
        string? source = null,
        RecipeMediaItemDto[]? mediaItems = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateRecipeAsync(string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipeId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteRecipeAsync(Guid recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipeId"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateRecipeNameAsync(Guid recipeId,
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipeId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<Guid>> GetImagesByRecipeIdAsync(Guid recipeId,
        CancellationToken cancellationToken);
}
