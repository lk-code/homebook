using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class SavingGoalsRepository(
    IDbContextFactory<AppDbContext> factory,
    ILogger<SavingGoalsRepository> logger)
    : ISavingGoalsRepository
{
    /// <inheritdoc />
    public async Task<IEnumerable<SavingGoal>> GetAllSavingGoalsAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving saving goals");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        List<SavingGoal> entities = await dbContext.Set<SavingGoal>()
            .Where(sg => sg.UserId == userId)
            .ToListAsync(cancellationToken);

        return entities;
    }

    public async Task<SavingGoal?> GetByIdAsync(Guid userId,
        Guid entityId,
        CancellationToken cancellationToken,
        AppDbContext? appDbContext = null)
    {
        logger.LogDebug("Retrieving saving goal");

        if (appDbContext is null)
        {
            await using AppDbContext newDbContext = await factory.CreateDbContextAsync(cancellationToken);
            return await GetByIdAsync(userId, entityId, cancellationToken, newDbContext);
        }

        SavingGoal? entity = await appDbContext.Set<SavingGoal>()
            .Where(e => e.UserId == userId
                        && e.Id == entityId)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    public async Task<Guid> CreateOrUpdateAsync(Guid userId,
        SavingGoal entity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating or updating saving goal");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        SavingGoal? existing = await GetByIdAsync(userId,
            entity.Id,
            cancellationToken,
            dbContext);

        if (existing is null)
        {
            entity.UserId = userId;
            dbContext.SavingGoals.Add(entity);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task DeleteAsync(Guid userId,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting saving goal");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        await dbContext.Set<SavingGoal>()
            .Where(e => e.UserId == userId
                        && e.Id == entityId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
