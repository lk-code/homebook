using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Modules.Abstractions;

namespace HomeBook.Backend.ModuleCore;

/// <inheritdoc />
public class StorageBuilder : IStorageBuilder,
    IStorageBuilderDataAccessor
{
    private List<string> _storageScopesNames = [];

    /// <inheritdoc />
    public IStorageBuilder RegisterStorage(string storageScopeName)
    {
        _storageScopesNames.Add(storageScopeName);
        return this;
    }

    /// <inheritdoc />
    public string[] GetStorageScopeNames() => _storageScopesNames.ToArray();
}
