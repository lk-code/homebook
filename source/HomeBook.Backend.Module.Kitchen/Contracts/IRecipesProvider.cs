using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Module.Kitchen.Models;

namespace HomeBook.Backend.Module.Kitchen.Contracts;

public interface IRecipesProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="searchFilter"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RecipeResultDto[]> GetRecipesAsync(string searchFilter,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RecipeResultDto?> GetRecipeByIdAsync(Guid id,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="requestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid> CreateOrUpdateAsync(RecipeRequestDto requestDto,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RecipeIngredient?> GetIngredientByNameAsync(string name,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid> CreateIngredientAsync(string name,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(Guid id,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateNameAsync(Guid id,
        string name,
        CancellationToken cancellationToken);

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid[]> GetImagesByRecipeIdAsync(Guid id,
        CancellationToken cancellationToken);
}
