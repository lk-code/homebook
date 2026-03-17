namespace HomeBook.Frontend.Modules.Abstractions;

public interface IFileStorageRegistration
{
    Task<Guid?> GetScopeIdForModuleAsync(IModule module,
        string scopeName,
        CancellationToken cancellationToken = default);
}
