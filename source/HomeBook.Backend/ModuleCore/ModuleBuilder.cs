using System.Reflection;
using HomeBook.Backend.Modules.Abstractions;
using HomeBook.Backend.Options;

namespace HomeBook.Backend.ModuleCore;

public class ModuleBuilder(
    HomeBookOptions homeBookOptions,
    IServiceCollection serviceCollection,
    IConfiguration configuration)
{
    private readonly List<string> _searchEnabledModules = [];

    public IReadOnlyList<string> SearchEnabledModules => _searchEnabledModules.AsReadOnly();

    /// <summary>
    /// adds a module to the service collection if the module is enabled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ModuleBuilder AddModule<T>() where T : class, IModule
    {
        string moduleId = typeof(T).FullName
                          ?? throw new InvalidOperationException("Module type must have a full name.");

        // register the module
        RegisterModule<T>(moduleId);

        return this;
    }

    private void RegisterModule<T>(string moduleId) where T : class, IModule
    {
        // register the IModule itself
        serviceCollection.AddSingleton<IModule, T>();
        serviceCollection.AddKeyedSingleton<IModule, T>(moduleId);

        // implements the Module the IBackendModuleServiceRegistrar interface?
        if (typeof(IBackendModuleServiceRegistrar).IsAssignableFrom(typeof(T)))
        {
            MethodInfo? method = typeof(T).GetMethod(
                nameof(IBackendModuleServiceRegistrar.RegisterServices),
                BindingFlags.Public | BindingFlags.Static
            );
            method?.Invoke(null, [serviceCollection, configuration]);
        }

        if (typeof(IBackendModuleSearchRegistrar).IsAssignableFrom(typeof(T)))
            _searchEnabledModules.Add(moduleId);
    }
}
