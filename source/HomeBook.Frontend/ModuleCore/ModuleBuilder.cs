using System.Reflection;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.Modules.Abstractions;
using HomeBook.Frontend.Options;

namespace HomeBook.Frontend.ModuleCore;

public class ModuleBuilder(
    HomeBookOptions homeBookOptions,
    IServiceCollection serviceCollection,
    IConfiguration configuration)
{
    private Dictionary<string, IWidgetBuilder> _registeredWidgets = new();
    private Dictionary<string, Type> _registeredSearchHandlerResultTemplates = new();
    private readonly List<SearchHandlerResultTemplateRegistration> _searchHandlerRegistrations = [];

    public Dictionary<string, Type> GetSearchHandlerResultTemplates() => _registeredSearchHandlerResultTemplates;
    public List<SearchHandlerResultTemplateRegistration> GetSearchHandlerRegistrations() => _searchHandlerRegistrations;

    /// <summary>
    /// adds a module to the service collection if the module is enabled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ModuleBuilder AddModule<T>() where T : class, IModule
    {
        // IFeatureManager featureManager = serviceCollection
        //     .BuildServiceProvider()
        //     .GetRequiredService<IFeatureManager>();

        string moduleId = typeof(T).FullName
                          ?? throw new InvalidOperationException("Module type must have a full name.");

        // bool isEnabled = featureManager.IsEnabledAsync(moduleId).GetAwaiter().GetResult();
        // if (!isEnabled)
        // {
        //     return this;
        // }

        // register the module
        RegisterModule<T>(moduleId);

        // register the modules widgets
        IWidgetBuilder widgetBuilder = new WidgetBuilder<T>();
        RegisterModuleWidgets<T>(widgetBuilder, moduleId);

        // create the startmenu items
        CreateStartMenuItems<T>(homeBookOptions.StartMenuBuilder, moduleId);

        // register the module search
        RegisterModuleSearch<T>(moduleId);

        _registeredWidgets.Add(moduleId, widgetBuilder);

        return this;
    }

    private void RegisterModuleSearch<T>(string moduleId) where T : class, IModule
    {
        ISearchHandlerResultTemplateBuilder searchHandlerResultTemplateBuilder = new SearchHandlerResultTemplateBuilder();

        // implements the Module the IModuleWidgetRegistration interface?
        if (!typeof(IModuleSearchRegistration).IsAssignableFrom(typeof(T)))
            return;

        // call the RegisterWidgets method in the module
        MethodInfo? method = typeof(T).GetMethod(
            nameof(IModuleSearchRegistration.RegisterSearch),
            BindingFlags.Public | BindingFlags.Static
        );
        method?.Invoke(null, [searchHandlerResultTemplateBuilder, configuration]);

        ISearchHandlerResultTemplateAccessor accessor = (ISearchHandlerResultTemplateAccessor)searchHandlerResultTemplateBuilder;
        Dictionary<string, Type> searchHandlerResultTemplates = accessor.GetSearchHandlerResultTemplates();

        _registeredSearchHandlerResultTemplates = _registeredSearchHandlerResultTemplates.Concat(searchHandlerResultTemplates)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private void CreateStartMenuItems<T>(IStartMenuBuilder startMenuBuilder,
        string moduleId) where T : class, IModule
    {
        // implements the Module the IModuleStartMenuRegistration interface?
        if (!typeof(IModuleStartMenuRegistration).IsAssignableFrom(typeof(T)))
            return;

        // call the RegisterWidgets method in the module
        startMenuBuilder.WithModule(moduleId);
        MethodInfo? method = typeof(T).GetMethod(
            nameof(IModuleStartMenuRegistration.RegisterStartMenuItems),
            BindingFlags.Public | BindingFlags.Static
        );
        method?.Invoke(null, [startMenuBuilder, configuration]);
    }

    private void RegisterModuleWidgets<T>(IWidgetBuilder widgetBuilder,
        string moduleId) where T : class, IModule
    {
        // implements the Module the IModuleWidgetRegistration interface?
        if (!typeof(IModuleWidgetRegistration).IsAssignableFrom(typeof(T)))
            return;

        // call the RegisterWidgets method in the module
        MethodInfo? method = typeof(T).GetMethod(
            nameof(IModuleWidgetRegistration.RegisterWidgets),
            BindingFlags.Public | BindingFlags.Static
        );
        method?.Invoke(null, [widgetBuilder, configuration]);
    }

    private void RegisterModule<T>(string moduleId) where T : class, IModule
    {
        // register the IModule itself
        serviceCollection.AddSingleton<IModule, T>();
        serviceCollection.AddKeyedSingleton<IModule, T>(moduleId);

        // implements the Module the IModuleDependencyRegistration interface?
        if (!typeof(IModuleDependencyRegistration).IsAssignableFrom(typeof(T)))
            return;

        // call the RegisterServices method in the module
        MethodInfo? method = typeof(T).GetMethod(
            "RegisterServices",
            BindingFlags.Public | BindingFlags.Static
        );
        method?.Invoke(null, [serviceCollection, configuration]);
    }

    public IWidgetBuilder GetWidgetBuilder(string moduleId) => _registeredWidgets[moduleId];

    public void GenerateSearchHandlerResultTemplateRegistration()
    {
        Dictionary<string, Type> searchHandlerResultTemplates = GetSearchHandlerResultTemplates();
        foreach (KeyValuePair<string, Type> searchHandlerResultTemplate in searchHandlerResultTemplates)
        {
            serviceCollection.AddScoped(typeof(ISearchHandlerResultTemplate), searchHandlerResultTemplate.Value);
            _searchHandlerRegistrations.Add(new SearchHandlerResultTemplateRegistration(searchHandlerResultTemplate.Key,
                searchHandlerResultTemplate.Value));
        }
    }
}
