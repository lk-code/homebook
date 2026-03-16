using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;

namespace HomeBook.Backend.Core.Storage;

/// <inheritdoc/>
public class StorageFileSystemProvider(
    IStorageModuleRegistrationRepository repository,
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

        await Task.CompletedTask;
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
    public async Task WriteFileAllBytesAsync(Guid scopeId,
        string filename,
        byte[] content,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fullFilePath = GetFullStorageFilePath(scopeId, filename);

        await fileSystemService.FileWriteAllBytesAsync(fullFilePath, content, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task WriteFileAllTextAsync(Guid scopeId,
        string filename,
        string content,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string fullFilePath = GetFullStorageFilePath(scopeId, filename);

        await fileSystemService.FileWriteAllTextAsync(fullFilePath, content, cancellationToken);
    }
}
