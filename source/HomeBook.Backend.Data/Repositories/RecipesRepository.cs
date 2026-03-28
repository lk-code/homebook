using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class RecipesRepository(
    IDbContextFactory<AppDbContext> factory,
    IStringNormalizer stringNormalizer,
    ILogger<RecipesRepository> logger)
    : IRecipesRepository
{
    public async Task<IEnumerable<Recipe>> GetAsync(string? searchFilter,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving recipes");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        var baseQuery = dbContext.Set<Recipe>()
            .Include(r => r.Recipe2MediaItems);

        // return all without filter
        if (string.IsNullOrWhiteSpace(searchFilter))
            return await baseQuery
                .ToListAsync(cancellationToken);

        // return with filter
        string normalizedFilter = stringNormalizer.Normalize(searchFilter);
        List<Recipe> entities = await baseQuery
            .Where(e => e.NormalizedName.Contains(normalizedFilter))
            .ToListAsync(cancellationToken);

        return entities;
    }

    /// <inheritdoc />
    public async Task<Recipe?> GetByIdAsync(Guid entityId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null)
    {
        logger.LogDebug("Retrieving recipe");

        if (appDbContext is null)
        {
            await using AppDbContext newDbContext = await factory.CreateDbContextAsync(cancellationToken);
            return await GetByIdAsync(entityId,
                cancellationToken,
                newDbContext);
        }

        Recipe? entity = await appDbContext.Set<Recipe>()
            .Include(r => r.Recipe2MediaItems.OrderBy(x => x.Index))
            .Include(r => r.Recipe2RecipeIngredients)
            .ThenInclude(ri => ri.RecipeIngredient)
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == entityId, cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateOrUpdateAsync(Recipe entity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating or updating recipe");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        bool exists = await dbContext.Recipes
            .AnyAsync(r => r.Id == entity.Id, cancellationToken);

        if (!exists)
        {
            dbContext.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // 1. update Recipe
            await dbContext.Recipes
                .Where(u => u.Id == entity.Id)
                .ExecuteUpdateAsync(x => x
                        .SetProperty(u => u.Name, entity.Name)
                        .SetProperty(u => u.NormalizedName, stringNormalizer.Normalize(entity.Name))
                        .SetProperty(u => u.Description, entity.Description)
                        .SetProperty(u => u.DurationWorkingMinutes, entity.DurationWorkingMinutes)
                        .SetProperty(u => u.DurationCookingMinutes, entity.DurationCookingMinutes)
                        .SetProperty(u => u.DurationRestingMinutes, entity.DurationRestingMinutes)
                        .SetProperty(u => u.CaloriesKcal, entity.CaloriesKcal)
                        .SetProperty(u => u.Servings, entity.Servings)
                        .SetProperty(u => u.Comments, entity.Comments)
                        .SetProperty(u => u.Source, entity.Source)
                        .SetProperty(u => u.UserId, entity.UserId),
                    cancellationToken: cancellationToken);

            // 2. update related data
            await ReplaceMediaRelationsAsync(dbContext,
                entity,
                cancellationToken);
            await UpdateIngredientRelationsAsync(dbContext,
                entity,
                cancellationToken);
            await ReplaceStepsAsync(dbContext,
                entity,
                cancellationToken);

            // 3. save changes
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid entityId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting recipe");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        await dbContext.Set<Recipe>()
            .Where(e => e.Id == entityId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Recipe2RecipeIngredient> CreateOrUpdateAsync(Recipe2RecipeIngredient entity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating or updating recipe ingredient relation");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        Recipe2RecipeIngredient? existing = await GetAsync(entity.RecipeId,
            entity.IngredientId,
            cancellationToken,
            dbContext);

        if (existing is null)
        {
            dbContext.Add(entity);
        }
        else
        {
            await dbContext.Recipe2RecipeIngredients
                .Where(u => u.RecipeId == entity.RecipeId
                            && u.IngredientId == entity.IngredientId)
                .ExecuteUpdateAsync(x => x
                        .SetProperty(u => u.Quantity, entity.Quantity)
                        .SetProperty(u => u.Unit, entity.Unit),
                    cancellationToken: cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public async Task<Recipe2RecipeIngredient?> GetAsync(Guid recipeId,
        Guid ingredientId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null)
    {
        logger.LogDebug("Retrieving recipe ingredient relation");

        if (appDbContext is null)
        {
            await using AppDbContext newDbContext = await factory.CreateDbContextAsync(cancellationToken);
            return await GetAsync(recipeId, ingredientId, cancellationToken, newDbContext);
        }

        Recipe2RecipeIngredient? entity = await appDbContext.Set<Recipe2RecipeIngredient>()
            .Include(r2ri => r2ri.RecipeIngredient)
            .Include(r2ri => r2ri.Recipe)
            .FirstOrDefaultAsync(r2ri => r2ri.RecipeId == recipeId
                                         && r2ri.IngredientId == ingredientId,
                cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<RecipeStep> CreateRecipeStepAsync(RecipeStep entity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating recipe step");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        dbContext.RecipeSteps.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateRecipeNameAsync(Guid id,
        string name,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating recipe name");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        await dbContext.Recipes
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(x => x
                    .SetProperty(r => r.Name, name)
                    .SetProperty(r => r.NormalizedName, stringNormalizer.Normalize(name)),
                cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid[]> GetImagesByRecipeIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving recipe image identifiers");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        Guid[]? entities = await dbContext.Recipes
            .Where(r => r.Id == id)
            .Select(r => r.Recipe2MediaItems
                .OrderBy(x => x.Index)
                .Select(x => x.MediaItemId)
                .ToArray())
            .SingleOrDefaultAsync(cancellationToken);

        if (entities is null)
            throw new EntityNotFoundException($"Recipe '{id}' was not found.");

        return entities;
    }

    private static async Task ReplaceMediaRelationsAsync(AppDbContext dbContext,
        Recipe entity,
        CancellationToken cancellationToken)
    {
        await dbContext.Recipe2MediaItems
            .Where(x => x.RecipeId == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (!entity.Recipe2MediaItems.Any())
            return;

        dbContext.Recipe2MediaItems.AddRange(entity.Recipe2MediaItems
            .OrderBy(x => x.Index)
            .Select(mediaItem => new Recipe2MediaItems
            {
                RecipeId = entity.Id,
                MediaItemId = mediaItem.MediaItemId,
                Index = mediaItem.Index
            }));
    }

    private static async Task UpdateIngredientRelationsAsync(AppDbContext dbContext,
        Recipe entity,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, Recipe2RecipeIngredient> requestedIngredientsById = entity.Recipe2RecipeIngredients
            .GroupBy(x => x.IngredientId)
            .ToDictionary(group => group.Key, group => group.Last());

        List<Recipe2RecipeIngredient> existingRelations = await dbContext.Recipe2RecipeIngredients
            .AsNoTracking()
            .Where(x => x.RecipeId == entity.Id)
            .ToListAsync(cancellationToken);

        HashSet<Guid> existingIngredientIds = existingRelations
            .Select(x => x.IngredientId)
            .ToHashSet();

        Guid[] ingredientIdsToRemove = existingIngredientIds
            .Except(requestedIngredientsById.Keys)
            .ToArray();
        if (ingredientIdsToRemove.Length > 0)
        {
            await dbContext.Recipe2RecipeIngredients
                .Where(x => x.RecipeId == entity.Id && ingredientIdsToRemove.Contains(x.IngredientId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        foreach (Recipe2RecipeIngredient existingRelation in existingRelations)
        {
            if (!requestedIngredientsById.TryGetValue(existingRelation.IngredientId, out Recipe2RecipeIngredient? requestedRelation))
                continue;

            if (existingRelation.Quantity == requestedRelation.Quantity
                && existingRelation.Unit == requestedRelation.Unit)
                continue;

            await dbContext.Recipe2RecipeIngredients
                .Where(x => x.RecipeId == entity.Id && x.IngredientId == existingRelation.IngredientId)
                .ExecuteUpdateAsync(x => x
                        .SetProperty(r => r.Quantity, requestedRelation.Quantity)
                        .SetProperty(r => r.Unit, requestedRelation.Unit),
                    cancellationToken: cancellationToken);
        }

        Guid[] ingredientIdsToAdd = requestedIngredientsById.Keys
            .Except(existingIngredientIds)
            .ToArray();
        if (ingredientIdsToAdd.Length > 0)
        {
            dbContext.Recipe2RecipeIngredients.AddRange(ingredientIdsToAdd.Select(ingredientId =>
            {
                Recipe2RecipeIngredient requestedRelation = requestedIngredientsById[ingredientId];
                return new Recipe2RecipeIngredient
                {
                    RecipeId = entity.Id,
                    IngredientId = ingredientId,
                    Quantity = requestedRelation.Quantity,
                    Unit = requestedRelation.Unit
                };
            }));
        }
    }

    private static async Task ReplaceStepsAsync(AppDbContext dbContext,
        Recipe entity,
        CancellationToken cancellationToken)
    {
        await dbContext.RecipeSteps
            .Where(x => x.RecipeId == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (!entity.Steps.Any())
            return;

        dbContext.RecipeSteps.AddRange(entity.Steps.Select(step => new RecipeStep
        {
            RecipeId = entity.Id,
            Position = step.Position,
            Description = step.Description,
            TimerDurationInSeconds = step.TimerDurationInSeconds
        }));
    }
}
