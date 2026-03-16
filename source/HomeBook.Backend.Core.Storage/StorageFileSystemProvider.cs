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
            return;

        string storagePath = Path.Combine(applicationPathProvider.StorageDirectory, scopeId.ToString());

        bool storageDirectoryAlreadyExists = fileSystemService.DirectoryExists(storagePath);
        if (!storageDirectoryAlreadyExists)
            fileSystemService.CreateDirectory(storagePath);
    }

    /// <inheritdoc/>
    public async Task<Guid?> GetScopeIdByFullName(string fullScopeName,
        CancellationToken cancellationToken) =>
        (await repository.GetByFullScopeNameAsync(fullScopeName,
            cancellationToken))?.Id;

    /// <inheritdoc/>
    public async Task DeleteFileAsync(Guid scopeId,
        string filename,
        CancellationToken cancellationToken)
    {
        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID cannot be empty", nameof(scopeId));

        string storagePath = Path.Combine(applicationPathProvider.StorageDirectory, scopeId.ToString());
        string fullFilePath = Path.Combine(storagePath, filename);

        fileSystemService.DeleteFile(fullFilePath);

        await Task.CompletedTask;
    }
}
