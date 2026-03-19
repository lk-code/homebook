using System.Linq.Expressions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class IngredientRepository(
    IDbContextFactory<AppDbContext> factory,
    IStringNormalizer stringNormalizer) : IIngredientRepository
{
    /// <inheritdoc />
    public async Task<Guid> CreateOrUpdateAsync(RecipeIngredient entity,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        entity.Normalize(stringNormalizer);

        if (entity.Id != Guid.Empty)
        {
            RecipeIngredient? existingById = await GetByIdAsync(entity.Id,
                cancellationToken,
                dbContext);

            if (existingById is not null)
            {
                await dbContext.RecipeIngredients
                    .Where(u => u.Id == entity.Id)
                    .ExecuteUpdateAsync(s => s
                            .SetProperty(u => u.Name, entity.Name)
                            .SetProperty(u => u.NormalizedName, entity.NormalizedName),
                        cancellationToken: cancellationToken);

                return entity.Id;
            }
        }

        RecipeIngredient? existing = await GetByName(entity.Name,
            cancellationToken,
            dbContext);

        if (existing is null)
        {
            dbContext.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        return existing.Id;
    }

    /// <inheritdoc />
    public async Task<RecipeIngredient?> GetByIdAsync(Guid entityId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null)
    {
        if (appDbContext is null)
        {
            await using AppDbContext newDbContext = await factory.CreateDbContextAsync(cancellationToken);
            return await GetByIdAsync(entityId, cancellationToken, newDbContext);
        }

        RecipeIngredient? entity = await appDbContext.Set<RecipeIngredient>()
            .FirstOrDefaultAsync(ri => ri.Id == entityId, cancellationToken);

        return entity;
    }

    /// <inheritdoc />
    public async Task<RecipeIngredient?> GetByName(string name,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null)
    {
        if (appDbContext is null)
        {
            await using AppDbContext newDbContext = await factory.CreateDbContextAsync(cancellationToken);
            return await GetByName(name, cancellationToken, newDbContext);
        }

        string normalizedName = stringNormalizer.Normalize(name);

        RecipeIngredient? entity = await appDbContext.Set<RecipeIngredient>()
            .FirstOrDefaultAsync(ri => ri.NormalizedName == normalizedName, cancellationToken);

        return entity;
    }
}
