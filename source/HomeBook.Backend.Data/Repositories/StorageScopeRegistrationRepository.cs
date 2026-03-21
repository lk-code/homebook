using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc/>
public class StorageScopeRegistrationRepository(
    IDbContextFactory<AppDbContext> factory,
    ILogger<StorageScopeRegistrationRepository> logger)
    : IStorageScopeRegistrationRepository
{
    /// <inheritdoc/>
    public async Task<List<StorageScopeRegistration>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all registered storage scopes");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        List<StorageScopeRegistration> entities = await dbContext.Set<StorageScopeRegistration>()
            .ToListAsync(cancellationToken);

        return entities;
    }

    /// <inheritdoc/>
    public async Task<StorageScopeRegistration?> GetByFullScopeNameAsync(string fullScopeName,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving storage scope by name");

        string scopeNameNormalized = fullScopeName.ToLowerInvariant();
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        StorageScopeRegistration? entity = await dbContext.Set<StorageScopeRegistration>()
            .Where(e => e.Name == scopeNameNormalized)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc/>
    public async Task<StorageScopeRegistration?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving storage scope");

        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        StorageScopeRegistration? entity = await dbContext.Set<StorageScopeRegistration>()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc/>
    public async Task<Guid> AddScopeAsync(string fullScopeName,
        string moduleKey,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding storage scope");

        StorageScopeRegistration? existing = await GetByFullScopeNameAsync(fullScopeName,
            cancellationToken);

        if (existing is not null)
        {
            throw new EntityExistsException($"Storage Scope for '{moduleKey}' already exists.");
        }

        string scopeNameNormalized = fullScopeName.ToLowerInvariant();
        string moduleKeyNormalized = moduleKey.ToLowerInvariant();
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        StorageScopeRegistration entity = new()
        {
            Name = scopeNameNormalized,
            ModuleKey = moduleKeyNormalized
        };

        dbContext.StorageScopeRegistrations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
