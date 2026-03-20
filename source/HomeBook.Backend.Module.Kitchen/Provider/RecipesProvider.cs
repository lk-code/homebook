using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Exceptions;
using HomeBook.Backend.Module.Kitchen.Contracts;
using HomeBook.Backend.Module.Kitchen.Exceptions;
using HomeBook.Backend.Module.Kitchen.Mappings;
using HomeBook.Backend.Module.Kitchen.Models;
using HomeBook.Backend.Modules.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Module.Kitchen.Provider;

/// <inheritdoc/>
public class RecipesProvider(
    ILogger<RecipesProvider> logger,
    IRecipesRepository recipesRepository,
    IIngredientRepository ingredientRepository,
    IStorageProvider storageProvider,
    IMediaProvider mediaProvider,
    [FromKeyedServices("HomeBook.Backend.Module.Kitchen.Module")]
    IModule module) : IRecipesProvider
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

        // get mediaIds which should be deleted because its not needed
        Guid[] mediaIdsToDelete = [];
        Guid[] originMediaIds = [];
        Guid[] currentMediaIds = [];
        if (requestDto.Id.HasValue)
        {
            originMediaIds = await GetImagesByRecipeIdAsync(requestDto.Id.Value,
                cancellationToken);
            currentMediaIds = requestDto.MediaItems
                .Select(x => x.MediaItemId)
                .ToArray();
            mediaIdsToDelete = originMediaIds
                .Except(currentMediaIds)
                .ToArray();
        }

        Recipe entity = requestDto.ToEntity();
        entity.Recipe2MediaItems = CreateMediaRelations(requestDto);
        entity.Recipe2RecipeIngredients = await CreateIngredientRelationsAsync(requestDto,
            cancellationToken);

        // TODO: validate entity

        Guid entityId = await recipesRepository
            .CreateOrUpdateAsync(entity,
                cancellationToken);

        // delete not needed images
        if (mediaIdsToDelete.Any())
        {
            string moduleKey = module.Key;
            string scopeName = "RecipeImages";
            Guid? recipeImagesStorageScopeId = await storageProvider.GetScopeIdByFullName($"{moduleKey}.{scopeName}",
                cancellationToken);

            foreach (Guid mediaIdToDelete in mediaIdsToDelete)
            {
                string? mediaItemFilename = await mediaProvider.GetFilenameByIdAsync(mediaIdToDelete,
                    cancellationToken);
                if (mediaItemFilename is null
                    || !recipeImagesStorageScopeId.HasValue)
                    continue;

                await storageProvider.DeleteFileAsync(recipeImagesStorageScopeId.Value,
                    mediaItemFilename,
                    cancellationToken);
            }
        }

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

    /// <inheritdoc/>
    public async Task<Guid[]> GetImagesByRecipeIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return await recipesRepository.GetImagesByRecipeIdAsync(id,
                cancellationToken);
        }
        catch (EntityNotFoundException err)
        {
            throw new RecipeNotFoundException("Recipe not found", err);
        }
    }

    private static Recipe2MediaItems[] CreateMediaRelations(RecipeRequestDto requestDto)
    {
        return requestDto.MediaItems
            .OrderBy(x => x.Index)
            .GroupBy(x => x.MediaItemId)
            .Select(group => group.First())
            .Select((mediaItem, index) =>
            {
                Recipe2MediaItems relation = new()
                {
                    MediaItemId = mediaItem.MediaItemId,
                    Index = index
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
