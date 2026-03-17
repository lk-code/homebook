using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc/>
public class StorageScopeRegistrationRepository(IDbContextFactory<AppDbContext> factory)
    : IStorageScopeRegistrationRepository
{
    /// <inheritdoc/>
    public async Task<StorageScopeRegistration?> GetByFullScopeNameAsync(string fullScopeName,
        CancellationToken cancellationToken)
    {
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
