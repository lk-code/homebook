using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.Modules.Abstractions;
using HomeBook.Frontend.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace HomeBook.Frontend.ModuleCore;

public static class ModuleExtensions
{
    private static ModuleBuilder? _moduleBuilder;

    public static void AddModules(this WebAssemblyHostBuilder builder,
        HomeBookOptions homeBookOptions,
        Action<ModuleBuilder> builderAction)
    {
        builder.Services.AddModules(
            homeBookOptions,
            builder.Configuration,
            builderAction);
    }

    public static void AddModules(this IServiceCollection sc,
        HomeBookOptions hb,
        IConfiguration c,
        Action<ModuleBuilder> builderAction)
    {
        _moduleBuilder = new ModuleBuilder(hb, sc, c);
        builderAction(_moduleBuilder);

        _moduleBuilder.GenerateSearchHandlerResultTemplateRegistration();
        foreach (SearchHandlerResultTemplateRegistration registration in _moduleBuilder.GetSearchHandlerRegistrations())
        {
            sc.AddSingleton(registration);
        }
    }

    public static async Task RunModulesPostBuild(this WebAssemblyHost host)
    {
        CancellationToken cancellationToken = CancellationToken.None;

        await host.Services.RunSupportModulesPostBuild(host.Configuration);

        IStartupService startupService = host.Services.GetRequiredService<IStartupService>();
        await startupService.StartAsync(cancellationToken);
    }

    public static async Task RunSupportModulesPostBuild(this IServiceProvider sp,
        IConfiguration c)
    {
        IEnumerable<IModule> modules = sp.GetServices<IModule>();
        IWidgetFactory widgetFactory = sp.GetRequiredService<IWidgetFactory>();

        foreach (IModule module in modules)
        {
            if (_moduleBuilder is null)
                return;

            string moduleId = module.GetType().FullName
                              ?? throw new InvalidOperationException("Module type must have a full name.");

            IWidgetBuilder widgetBuilder = _moduleBuilder.GetWidgetBuilder(moduleId);
            widgetFactory.AddWidgetBuilder(moduleId, widgetBuilder);

            try
            {
                await module.InitializeAsync();
            }
            catch (NotImplementedException)
            {
            }
        }
    }
}
