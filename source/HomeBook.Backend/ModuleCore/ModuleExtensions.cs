using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Search;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Factories;
using HomeBook.Backend.Modules.Abstractions;
using HomeBook.Backend.Options;
using Npgsql;

namespace HomeBook.Backend.ModuleCore;

public static class ModuleExtensions
{
    private static ModuleBuilder? _moduleBuilder = null;
    private static SearchRegistrationFactory? _searchRegistrationFactory = null;

    /// <summary>
    /// use in Blazor Server
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="homeBookOptions"></param>
    /// <param name="builderAction"></param>
    public static void AddModules(this WebApplicationBuilder builder,
        HomeBookOptions homeBookOptions,
        Action<ModuleBuilder> builderAction)
    {
        builder.Services.AddModules(
            homeBookOptions,
            builder.Configuration,
            builderAction);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="sc"></param>
    /// <param name="hb"></param>
    /// <param name="c"></param>
    /// <param name="builderAction"></param>
    public static void AddModules(this IServiceCollection sc,
        HomeBookOptions hb,
        IConfiguration c,
        Action<ModuleBuilder> builderAction)
    {
        _searchRegistrationFactory = new();
        _moduleBuilder = new ModuleBuilder(hb, sc, c);
        builderAction(_moduleBuilder);
    }

    /// <summary>
    /// use in Blazor Server
    /// </summary>
    /// <param name="host"></param>
    public static async Task RunModulesPostBuild(this WebApplication host)
    {
        CancellationToken cancellationToken = CancellationToken.None;

        ISearchRegistrationInitiator searchRegistrationInitiator = host.Services
            .GetRequiredService<ISearchRegistrationInitiator>();
        searchRegistrationInitiator.AddServiceProvider(host.Services);

        // register the search provider with modules
        // sc.AddSingleton<ISearchRegistrationFactory>(x =>
        // {
        //     _searchRegistrationFactory.AddServiceProvider(x);
        //     return _searchRegistrationFactory!;
        // });

        await host.RunModulesPostBuild(host.Services,
            host.Configuration);

        // call startup service if needed
    }

    /// <summary>
    /// general post build logic
    /// </summary>
    /// <param name="host"></param>
    /// <param name="sp"></param>
    /// <param name="c"></param>
    public static async Task RunModulesPostBuild(this WebApplication host,
        IServiceProvider sp,
        IConfiguration c)
    {
        if (_moduleBuilder is null)
            return;

        // register search enabled modules in search registration factory
        ISearchRegistrationInitiator searchRegistrationInitiator = sp
            .GetRequiredService<ISearchRegistrationInitiator>();
        _moduleBuilder.RegisterModulesInSearchFactory(searchRegistrationInitiator);

        IEnumerable<IModule> modules = sp.GetServices<IModule>();

        // TODO: check that every module key is unique. otherwise stop registering modules! this is for safety because otherwise a scam module can access data from another module with its key

        // initialize all modules
        foreach (IModule module in modules)
        {
            // TODO: ensure that module.key contains only a-z and 0-9

            // register endpoints
            try
            {
                await host.RegisterEndpointsForModuleAsync(module);
            }
            catch (NotImplementedException)
            {
                // do nothing
            }

            // register storage
            try
            {
                await host.RegisterStorageForModuleAsync(module);
            }
            catch (NotImplementedException)
            {
                // do nothing
            }
            catch (Exception err)
            {
                int i = 0;
                // this may happen if the app is started after an update and the migrations are not executed
            }

            // call the initialization logic
            try
            {
                await module.InitializeAsync();
            }
            catch (NotImplementedException)
            {
                // do nothing
            }
        }
    }

    public static async Task RegisterStorageForModuleAsync(this WebApplication host,
        IModule module)
    {
        if (module is not IBackendModuleStorageRegistrar registrar)
            return;

        CancellationToken cancellationToken = CancellationToken.None;

        using IServiceScope scope = host.Services.CreateScope();
        IStorageProvider storageProvider = scope.ServiceProvider.GetRequiredService<IStorageProvider>();
        IBackendModuleStorageRegistrar storageRegistrar = registrar;

        IConfiguration configuration = host.Configuration;

        IStorageBuilder builder = new StorageBuilder();
        storageRegistrar.RegisterStorage(builder, configuration);

        IStorageBuilderDataAccessor accessor = (IStorageBuilderDataAccessor)builder;

        string[] scopeNames = accessor.GetStorageScopeNames();
        foreach (string scopeName in scopeNames)
        {
            string fullScopeName = $"{module.Key}.{scopeName}";

            bool isScopeRegistered = await storageProvider.IsScopeRegisteredAsync(fullScopeName,
                cancellationToken);
            Guid scopeId = Guid.Empty;
            if (!isScopeRegistered)
            {
                scopeId = await storageProvider.RegisterStorageScopeAsync(fullScopeName,
                    module.Key,
                    cancellationToken);
            }
            else
            {
                scopeId = (await storageProvider.GetScopeIdByFullName(fullScopeName,
                    cancellationToken))!.Value;
            }

            // TODO: create storage directory for scope-id like /data/storage/{guid} if doesnt exists
            await storageProvider.CreateStorageForScopeAsync(scopeId, cancellationToken);
        }
    }

    public static async Task RegisterEndpointsForModuleAsync(this WebApplication host,
        IModule module)
    {
        if (module is not IBackendModuleEndpointRegistrar registrar)
            return;
        IBackendModuleEndpointRegistrar endpointRegistrar = registrar;

        IConfiguration configuration = host.Configuration;

        // register endpoint group for module
        string endpointModuleGroupKey = module.Key.Replace(".", "/").ToLower();
        RouteGroupBuilder moduleEndpointGroup = host.MapGroup($"/modules/{endpointModuleGroupKey}")
            .WithDescription(module.Description)
            .WithTags([
                module.Name
            ]);

        IEndpointBuilder builder = new EndpointBuilder(moduleEndpointGroup);
        endpointRegistrar.RegisterEndpoints(builder, configuration);

        IEndpointDataAccessor endpointDataAccessor = (IEndpointDataAccessor)builder;
    }
}
