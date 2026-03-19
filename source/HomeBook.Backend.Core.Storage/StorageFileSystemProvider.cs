using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;

namespace HomeBook.Backend.Core.Storage;

/// <inheritdoc/>
public class StorageFileSystemProvider(
    IStorageScopeRegistrationRepository repository,
    IMediaItemRepository mediaItemRepository,
    IApplicationPathProvider applicationPathProvider,
    IFileSystemService fileSystemService) : IStorageProvider
{
    /// <inheritdoc/>
    public async Task<bool> IsScopeRegisteredAsync(string fullScopeName,
        CancellationToken cancellationToken) =>
        (await repository.GetByFullScopeNameAsync(fullScopeName,
            cancellationToken) is not null);

    /// <inheritdoc/>
    public async Task<bool> IsScopeRegisteredAsync(Guid scopeId,
        CancellationToken cancellationToken) =>
        (await repository.GetByIdAsync(scopeId,
            cancellationToken) is not null);

    /// <inheritdoc/>
    public async Task<Guid> RegisterStorageScopeAsync(string fullScopeName,
        string moduleKey,
        CancellationToken cancellationToken) =>
        await repository.AddScopeAsync(fullScopeName,
            moduleKey,
            cancellationToken);

    /// <inheritdoc/>
    public async Task CreateStorageForScopeAsync(Guid scopeId,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string storagePath = Path.Combine(applicationPathProvider.StorageDirectory, scopeId.ToString());

        bool storageDirectoryAlreadyExists = fileSystemService.DirectoryExists(storagePath);
        if (!storageDirectoryAlreadyExists)
            fileSystemService.CreateDirectory(storagePath);

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<Guid?> GetScopeIdByFullName(string fullScopeName,
        CancellationToken cancellationToken) =>
        (await repository.GetByFullScopeNameAsync(fullScopeName,
            cancellationToken))?.Id;

    private string GetFullStorageFilePath(Guid scopeId, string filename)
    {
        string storagePath = Path.Combine(applicationPathProvider.StorageDirectory, scopeId.ToString());
        string fullFilePath = Path.Combine(storagePath, filename);
        return fullFilePath;
    }

    /// <inheritdoc/>
    public async Task DeleteFileAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fullFilePath = GetFullStorageFilePath(scopeId, filename);

        fileSystemService.DeleteFile(fullFilePath);

        MediaItem? mediaItemId = await mediaItemRepository
            .GetMediaItemByFilenameAsync(scopeId, filename, cancellationToken);
        if (mediaItemId is null)
            return;

        await mediaItemRepository.DeleteMediaItemAsync(mediaItemId!.Id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetFileAllBytesAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fullFilePath = GetFullStorageFilePath(scopeId, filename);

        byte[] content = await fileSystemService.FileReadAllBytesAsync(fullFilePath, cancellationToken);

        return content;
    }

    /// <inheritdoc/>
    public async Task<string> GetFileAllTextAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fullFilePath = GetFullStorageFilePath(scopeId, filename);

        string content = await fileSystemService.FileReadAllTextAsync(fullFilePath, cancellationToken);

        return content;
    }

    /// <inheritdoc/>
    public async Task<Guid> WriteFileAllBytesAsync(Guid scopeId,
        string originalFilename,
        byte[] content,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fileExt = Path.GetExtension(originalFilename);
        string internalFileName = $"{Guid.NewGuid()}{fileExt}";
        string fullFilePath = GetFullStorageFilePath(scopeId, internalFileName);

        await fileSystemService.FileWriteAllBytesAsync(fullFilePath, content, cancellationToken);

        MediaItem? mediaItem = await mediaItemRepository.GetMediaItemByFilenameAsync(scopeId,
            internalFileName,
            cancellationToken);
        if (mediaItem is not null)
            await mediaItemRepository.DeleteMediaItemAsync(mediaItem!.Id, cancellationToken);

        Guid mediaItemId = await mediaItemRepository.AddMediaItemAsync(scopeId,
            internalFileName,
            cancellationToken);

        return mediaItemId;
    }

    /// <inheritdoc/>
    public async Task<Guid> WriteFileAllTextAsync(Guid scopeId,
        string originalFilename,
        string content,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fileExt = Path.GetExtension(originalFilename);
        string internalFileName = $"{Guid.NewGuid()}{fileExt}";
        string fullFilePath = GetFullStorageFilePath(scopeId, internalFileName);

        await fileSystemService.FileWriteAllTextAsync(fullFilePath, content, cancellationToken);

        MediaItem? mediaItem = await mediaItemRepository.GetMediaItemByFilenameAsync(scopeId,
            internalFileName,
            cancellationToken);
        if (mediaItem is not null)
            await mediaItemRepository.DeleteMediaItemAsync(mediaItem!.Id, cancellationToken);

        Guid mediaItemId = await mediaItemRepository.AddMediaItemAsync(scopeId,
            internalFileName,
            cancellationToken);

        return mediaItemId;
    }
}
