using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class RecipesRepository(
    IDbContextFactory<AppDbContext> factory,
    IStringNormalizer stringNormalizer)
    : IRecipesRepository
{
    public async Task<IEnumerable<Recipe>> GetAsync(string? searchFilter,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        // return all without filter
        if (string.IsNullOrWhiteSpace(searchFilter))
            return await dbContext.Set<Recipe>()
                .ToListAsync(cancellationToken);

        // return with filter
        string normalizedFilter = stringNormalizer.Normalize(searchFilter);
        return await dbContext.Set<Recipe>()
            .Include(r => r.Recipe2MediaItems)
            .Where(e => e.NormalizedName.Contains(normalizedFilter))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Recipe?> GetByIdAsync(Guid entityId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null)
    {
        if (appDbContext is null)
        {
            await using AppDbContext newDbContext = await factory.CreateDbContextAsync(cancellationToken);
            return await GetByIdAsync(entityId, cancellationToken, newDbContext);
        }

        Recipe? entity = await appDbContext.Set<Recipe>()
            .Include(r => r.Recipe2MediaItems)
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
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        Recipe? existing = await GetByIdAsync(entity.Id,
            cancellationToken,
            dbContext);

        if (existing is null)
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

            // 2. update Steps and Ingredients
            // remove all existing Steps and Ingredients
            await dbContext.Recipe2MediaItems.Where(x => x.RecipeId == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.Recipe2RecipeIngredients.Where(x => x.RecipeId == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.RecipeSteps.Where(x => x.RecipeId == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);

            // insert new Steps and Ingredients
            dbContext.Recipe2MediaItems.AddRange(entity.Recipe2MediaItems);
            dbContext.Recipe2RecipeIngredients.AddRange(entity.Recipe2RecipeIngredients);
            dbContext.RecipeSteps.AddRange(entity.Steps);

            // 3. save changes
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid entityId,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        await dbContext.Set<Recipe>()
            .Where(e => e.Id == entityId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Recipe2RecipeIngredient> CreateOrUpdateAsync(Recipe2RecipeIngredient entity,
        CancellationToken cancellationToken)
    {
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
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        await dbContext.Recipes
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(x => x
                    .SetProperty(r => r.Name, name)
                    .SetProperty(r => r.NormalizedName, stringNormalizer.Normalize(name)),
                cancellationToken: cancellationToken);
    }
}
