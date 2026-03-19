using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Module.Kitchen.Contracts;
using HomeBook.Backend.Module.Kitchen.Mappings;
using HomeBook.Backend.Module.Kitchen.Models;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Module.Kitchen.Provider;

/// <inheritdoc/>
public class RecipesProvider(
    ILogger<RecipesProvider> logger,
    IRecipesRepository recipesRepository,
    IIngredientRepository ingredientRepository) : IRecipesProvider
{
    /// <inheritdoc/>
    public async Task<RecipeResultDto[]> GetRecipesAsync(string searchFilter,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving meals with search filter: {SearchFilter}",
            searchFilter);

        IEnumerable<Recipe> recipeEntities = await recipesRepository.GetAsync(searchFilter,
            cancellationToken);
        RecipeResultDto[] recipes = recipeEntities
            .Select(m => m.ToDto())
            .ToArray();

        return recipes;
    }

    /// <inheritdoc/>
    public async Task<RecipeResultDto?> GetRecipeByIdAsync(Guid id,
        CancellationToken cancellationToken) =>
        (await recipesRepository.GetByIdAsync(id,
            cancellationToken))?.ToDto();

    /// <inheritdoc/>
    public async Task<Guid> CreateOrUpdateAsync(RecipeRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        // TODO: validate dto

        Recipe entity = requestDto.ToEntity();
        entity.Recipe2MediaItems = CreateMediaRelations(requestDto);
        entity.Recipe2RecipeIngredients = await CreateIngredientRelationsAsync(requestDto,
            cancellationToken);

        // TODO: validate entity

        Guid entityId = await recipesRepository
            .CreateOrUpdateAsync(entity,
                cancellationToken);
        return entityId;
    }

    /// <inheritdoc/>
    public async Task<RecipeIngredient?> GetIngredientByNameAsync(string name,
        CancellationToken cancellationToken) =>
        await ingredientRepository.GetByName(name,
            cancellationToken);

    /// <inheritdoc/>
    public async Task<Guid> CreateIngredientAsync(string name,
        CancellationToken cancellationToken)
    {
        RecipeIngredient entity = new()
        {
            Name = name
        };

        Guid entityId = await ingredientRepository.CreateOrUpdateAsync(entity,
            cancellationToken);

        return entityId;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await recipesRepository.DeleteAsync(id,
            cancellationToken);

    /// <inheritdoc/>
    public async Task UpdateNameAsync(Guid id,
        string name,
        CancellationToken cancellationToken)
    {
        await recipesRepository.UpdateRecipeNameAsync(id,
            name,
            cancellationToken);
    }

    private static Recipe2MediaItems[] CreateMediaRelations(RecipeRequestDto requestDto)
    {
        return requestDto.MediaIds
            .Distinct()
            .Select(mediaId =>
            {
                Recipe2MediaItems relation = new()
                {
                    MediaItemId = mediaId
                };

                if (requestDto.Id.HasValue)
                    relation.RecipeId = requestDto.Id.Value;

                return relation;
            })
            .ToArray();
    }

    private async Task<Recipe2RecipeIngredient[]> CreateIngredientRelationsAsync(RecipeRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        List<Recipe2RecipeIngredient> relations = [];

        foreach (RecipeIngredientRequestDto ingredient in requestDto.Ingredients)
        {
            Guid ingredientId = await ingredientRepository.CreateOrUpdateAsync(new RecipeIngredient
                {
                    Name = ingredient.Name
                },
                cancellationToken);

            Recipe2RecipeIngredient? existingRelation = relations.FirstOrDefault(r => r.IngredientId == ingredientId);
            if (existingRelation is not null)
            {
                existingRelation.Quantity = ingredient.Quantity;
                existingRelation.Unit = ingredient.Unit;
                continue;
            }

            Recipe2RecipeIngredient relation = new()
            {
                IngredientId = ingredientId,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit
            };

            if (requestDto.Id.HasValue)
                relation.RecipeId = requestDto.Id.Value;

            relations.Add(relation);
        }

        return relations.ToArray();
    }
}
