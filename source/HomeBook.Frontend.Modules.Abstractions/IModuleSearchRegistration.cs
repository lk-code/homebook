using Microsoft.Extensions.Configuration;

namespace HomeBook.Frontend.Modules.Abstractions;

/// <summary>
///
/// </summary>
public interface IModuleSearchRegistration
{
    /// <summary>
    /// register widgets provided by the module
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    static abstract void RegisterSearch(ISearchHandlerResultTemplateBuilder builder,
        IConfiguration configuration);
}
