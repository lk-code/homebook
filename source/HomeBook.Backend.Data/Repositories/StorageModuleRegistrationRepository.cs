using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc/>
public class StorageModuleRegistrationRepository(IDbContextFactory<AppDbContext> factory)
    : IStorageModuleRegistrationRepository
{
    /// <inheritdoc/>
    public async Task<StorageModuleRegistration?> GetByFullScopeNameAsync(string fullScopeName,
        CancellationToken cancellationToken)
    {
        string scopeNameNormalized = fullScopeName.ToLowerInvariant();
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        StorageModuleRegistration? entity = await dbContext.Set<StorageModuleRegistration>()
            .Where(e => e.ScopeName == scopeNameNormalized)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc/>
    public async Task<StorageModuleRegistration?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        StorageModuleRegistration? entity = await dbContext.Set<StorageModuleRegistration>()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc/>
    public async Task<Guid> AddScopeAsync(string fullScopeName,
        string moduleKey,
        CancellationToken cancellationToken)
    {
        StorageModuleRegistration? existing = await GetByFullScopeNameAsync(fullScopeName,
            cancellationToken);

        if (existing is not null)
        {
            throw new EntityExistsException($"Storage Scope for '{moduleKey}' already exists.");
        }

        string scopeNameNormalized = fullScopeName.ToLowerInvariant();
        string moduleKeyNormalized = moduleKey.ToLowerInvariant();
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        StorageModuleRegistration entity = new()
        {
            ScopeName = scopeNameNormalized,
            ModuleKey = moduleKeyNormalized
        };

        dbContext.StorageModuleRegistrations.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
