using HomeBook.Frontend.Core.Icons;
using HomeBook.Frontend.Module.PlatformInfo.Resources;
using HomeBook.Frontend.Modules.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace HomeBook.Frontend.Module.PlatformInfo;

/// <summary>
/// the platform info module
/// </summary>
public class Module(IStringLocalizer<Strings> Loc)
    : IModule,
        IModuleWidgetRegistration,
        IModuleDependencyRegistration
{
    /// <inheritdoc />
    public string Name => Loc[nameof(Strings.ModuleName)];

    /// <inheritdoc />
    public string Description => Loc[nameof(Strings.ModuleDescription)];

    public string Key { get; } = "homebook.platforminfo";

    /// <inheritdoc />
    public string Author { get; } = "HomeBook";

    /// <inheritdoc />
    public Version Version => new("1.0.0");

    /// <inheritdoc />
    public string Icon { get; } = HomeBookIcons.Icons8.LiquidGlassColor.Help;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GetTranslation(string key, params object[] args) => Loc[key, args];

    public static void RegisterWidgets(IWidgetBuilder builder, IConfiguration configuration)
    {
        builder.AddWidget<Widgets.VersionWidget>();
    }

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}
