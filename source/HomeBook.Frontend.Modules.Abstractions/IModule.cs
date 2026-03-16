using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Frontend.Modules.Abstractions;

public interface IModule
{
    /// <summary>
    /// the display name of this module
    /// </summary>
    string Name { get; }

    /// <summary>
    /// the description of this module
    /// </summary>
    string Description { get; }

    /// <summary>
    /// the key of this module (used for endpoint grouping, etc.)
    /// </summary>
    string Key { get; }

    /// <summary>
    /// the author of this module
    /// </summary>
    string Author { get; }

    /// <summary>
    /// the version of this module
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// the display icon of this module
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// contains the initialization logic for the module.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    string GetTranslation(string key, params object[] args);
}
