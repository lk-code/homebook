using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Contracts;

public interface IInternalFileStorageRegistration : IFileStorageRegistration
{
    Task<Guid?> GetScopeIdForModuleAsync(string moduleKey,
        string scopeName,
        CancellationToken cancellationToken = default);
}
