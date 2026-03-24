using Microsoft.Extensions.Configuration;

namespace HomeBook.Backend.Modules.Abstractions;

/// <summary>
///
/// </summary>
public interface IBackendModuleSearchRegistrar
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="searchBuilder"></param>
    /// <param name="configuration"></param>
    static abstract void RegisterSearch(ISearchBuilder searchBuilder,
        IConfiguration configuration);
}
