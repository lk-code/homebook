using HomeBook.Backend.Data.Entities;

namespace HomeBook.Backend.Data.Contracts;

public interface IRecipesRepository
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="searchFilter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Recipe>> GetAsync(string? searchFilter,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="appDbContext"></param>
    /// <returns></returns>
    Task<Recipe?> GetByIdAsync(Guid entityId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid> CreateOrUpdateAsync(Recipe entity,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(Guid entityId,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Recipe2RecipeIngredient> CreateOrUpdateAsync(Recipe2RecipeIngredient entity,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="recipeId"></param>
    /// <param name="ingredientId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="appDbContext"></param>
    /// <returns></returns>
    Task<Recipe2RecipeIngredient?> GetAsync(Guid recipeId,
        Guid ingredientId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RecipeStep> CreateRecipeStepAsync(RecipeStep entity,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateRecipeNameAsync(Guid id,
        string name,
        CancellationToken cancellationToken);

    Task<Guid[]> GetImagesByRecipeIdAsync(Guid id,
        CancellationToken cancellationToken);
}
