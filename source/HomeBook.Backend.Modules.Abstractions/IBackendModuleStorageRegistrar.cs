using Microsoft.Extensions.Configuration;

namespace HomeBook.Backend.Modules.Abstractions;

public interface IBackendModuleStorageRegistrar
{
    /// <summary>
    /// register the storages for this module
    /// </summary>
    /// <param name="storageBuilder"></param>
    /// <param name="configuration"></param>
    void RegisterStorage(IStorageBuilder storageBuilder,
        IConfiguration configuration);
}
