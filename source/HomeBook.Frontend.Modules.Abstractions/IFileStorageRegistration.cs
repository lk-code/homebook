namespace HomeBook.Frontend.Modules.Abstractions;

public interface IFileStorageRegistration
{
    string FileExtForImages { get; }

    Task<Guid?> GetScopeIdForModuleAsync(IModule module,
        string scopeName,
        CancellationToken cancellationToken = default);
}
