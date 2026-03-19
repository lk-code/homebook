using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Repositories;

/// <inheritdoc/>
public class MediaItemRepository(IDbContextFactory<AppDbContext> factory)
    : IMediaItemRepository
{
    /// <inheritdoc/>
    public async Task<Guid> AddMediaItemAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        MediaItem? existing = await GetMediaItemByFilenameAsync(scopeId,
            filename,
            cancellationToken);

        if (existing is not null)
        {
            throw new EntityExistsException(
                $"Media item with filename '{filename}' already exists in scope '{scopeId}'.");
        }

        MediaItem entity = new()
        {
            StorageScopeId = scopeId,
            FileName = filename
        };

        dbContext.MediaItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    /// <inheritdoc/>
    public async Task<MediaItem?> GetMediaItemByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        MediaItem? entity = await dbContext.MediaItems.Where(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc/>
    public async Task<MediaItem?> GetMediaItemByFilenameAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        MediaItem? entity = await dbContext.MediaItems.Where(m => m.StorageScopeId == scopeId
                                                                  && m.FileName == filename)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    /// <inheritdoc/>
    public async Task DeleteMediaItemAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);

        await dbContext.MediaItems.Where(m => m.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string?> GetFilenameByIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        await using AppDbContext dbContext = await factory.CreateDbContextAsync(cancellationToken);
        string? entity = await dbContext.MediaItems
            .Where(m => m.Id == id)
            .Select(x => x.FileName)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }
}
