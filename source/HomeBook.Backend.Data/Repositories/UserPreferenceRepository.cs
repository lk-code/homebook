using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc />
public class UserPreferenceRepository(
    IDbContextFactory<AppDbContext> factory,
    ILogger<UserPreferenceRepository> logger) : IUserPreferenceRepository
{
    /// <inheritdoc />
    public async Task<UserPreference?> GetPreferenceForUserByKeyAsync(Guid userId,
        string key,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving user preference");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        UserPreference? userPreference = await dbContext.Set<UserPreference>()
            .FirstOrDefaultAsync(c => c.Key == key, cancellationToken);

        return userPreference;
    }

    /// <inheritdoc />
    public async Task SetPreferenceForUserByKeyAsync(UserPreference entity,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting user preference");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        UserPreference? existingEntity = await dbContext.Set<UserPreference>()
            .FirstOrDefaultAsync(c => c.UserId == entity.UserId
                                      && c.Key == entity.Key,
                cancellationToken);

        if (existingEntity is null)
        {
            dbContext.Add(entity);
        }
        else
        {
            existingEntity.Value = entity.Value;
            dbContext.Update(existingEntity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
