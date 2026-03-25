using HomeBook.Frontend.Core.Icons;
using HomeBook.Frontend.Module.Finances.Contracts;
using HomeBook.Frontend.Module.Finances.Factories;
using HomeBook.Frontend.Module.Finances.Resources;
using HomeBook.Frontend.Module.Finances.Services;
using HomeBook.Frontend.Modules.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace HomeBook.Frontend.Module.Finances;

/// <summary>
/// the finances module
/// </summary>
public class Module(IStringLocalizer<Strings> Loc)
    : IModule,
        IModuleWidgetRegistration,
        IModuleDependencyRegistration,
        IModuleStartMenuRegistration,
        IModuleSearchRegistration
{
    /// <inheritdoc />
    public string Name => Loc[nameof(Strings.ModuleName)];

    /// <inheritdoc />
    public string Description => Loc[nameof(Strings.ModuleDescription)];

    /// <inheritdoc />
    public string Key { get; } = "homebook.finances";

    /// <inheritdoc />
    public string Author { get; } = "HomeBook";

    /// <inheritdoc />
    public Version Version => new("1.0.0");

    /// <inheritdoc />
    public string Icon { get; } = HomeBookIcons.Icons8.LiquidGlassColor.Investment;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GetTranslation(string key, params object[] args) => Loc[key, args];

    /// <inheritdoc />
    public static void RegisterWidgets(IWidgetBuilder builder,
        IConfiguration configuration)
    {
        builder.AddWidget<Widgets.CurrentBudgetWidget>();
    }

    /// <inheritdoc />
    public static void RegisterServices(IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ISavingGoalService, SavingGoalService>();

        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
    }

    /// <inheritdoc />
    public static void RegisterStartMenuItems(IStartMenuBuilder builder,
        IConfiguration configuration)
    {
        builder.AddStartMenuTile(nameof(Strings.StartMenuItem_Overview_Title),
            nameof(Strings.StartMenuItem_Overview_Caption),
            "/Finances",
            HomeBookIcons.Icons8.Windows11.Filled.Graph,
            "var(--hb-color-denim)");
    }

    public static void RegisterSearch(ISearchHandlerResultTemplateBuilder builder, IConfiguration configuration)
    {
        builder.AddSearchHandlerResultTemplate<Search.Templates.SavingGoalsSearchHandlerResultTemplate>("HomeBook.Backend.Module.Finances.Module.SavingGoalSearchHandler");
    }
}
