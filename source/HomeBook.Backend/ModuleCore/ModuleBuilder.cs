using System.Reflection;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Search;
using HomeBook.Backend.Core.Search.Models;
using HomeBook.Backend.Modules.Abstractions;
using HomeBook.Backend.Options;

namespace HomeBook.Backend.ModuleCore;

public class ModuleBuilder(
    HomeBookOptions homeBookOptions,
    IServiceCollection serviceCollection,
    IConfiguration configuration)
{
    private readonly List<SearchHandlerRegistration> _searchHandlerRegistrations = [];
    public List<SearchHandlerRegistration> GetSearchHandlerRegistrations() => _searchHandlerRegistrations;

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
        RegisterModuleServices<T>();

        // implements the Module the IBackendModuleSearchRegistrar interface?
        RegisterModuleSearchHandler<T>(moduleId);
    }

    private void RegisterModuleSearchHandler<T>(string moduleId) where T : class, IModule
    {
        if (typeof(IBackendModuleSearchRegistrar).IsAssignableFrom(typeof(T)))
        {
            ISearchBuilder searchBuilder = new SearchBuilder();

            MethodInfo? method = typeof(T).GetMethod(
                nameof(IBackendModuleSearchRegistrar.RegisterSearch),
                BindingFlags.Public | BindingFlags.Static
            );
            method?.Invoke(null, [searchBuilder, configuration]);

            ISearchBuilderDataAccessor accessor = (ISearchBuilderDataAccessor)searchBuilder;
            IEnumerable<Type> searchHandlerTypes = accessor.GetRegisteredSearchHandlers();
            foreach (Type searchHandlerType in searchHandlerTypes)
            {
                serviceCollection.AddScoped(typeof(ISearchHandler), searchHandlerType);
                _searchHandlerRegistrations.Add(new SearchHandlerRegistration(moduleId, searchHandlerType));
            }
        }
    }

    private void RegisterModuleServices<T>() where T : class, IModule
    {
        if (typeof(IBackendModuleServiceRegistrar).IsAssignableFrom(typeof(T)))
        {
            MethodInfo? method = typeof(T).GetMethod(
                nameof(IBackendModuleServiceRegistrar.RegisterServices),
                BindingFlags.Public | BindingFlags.Static
            );
            method?.Invoke(null, [serviceCollection, configuration]);
        }
    }
}
