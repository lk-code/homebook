using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Module.Kitchen.Contracts;
using HomeBook.Backend.Module.Kitchen.Endpoints;
using HomeBook.Backend.Module.Kitchen.Provider;
using HomeBook.Backend.Modules.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Module.Kitchen;

public class Module : IModule,
    IBackendModuleEndpointRegistrar,
    IBackendModuleServiceRegistrar,
    IBackendModuleSearchRegistrar,
    IBackendModuleStorageRegistrar
{
    public string Name { get; } = "Kitchen Module";
    public string Description { get; } = "Provides kitchen and recipe management features";
    public string Key { get; } = "homebook.kitchen";
    public string Author { get; } = "HomeBook";
    public Version Version { get; } = new("1.0.0");

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public void RegisterEndpoints(IEndpointBuilder builder,
        IConfiguration configuration)
    {
        builder.MapRecipeEndpoints();
    }

    public static void RegisterServices(IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRecipesProvider, RecipesProvider>();
    }

    public async Task<SearchResult> SearchAsync(string query,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(3500, cancellationToken); // Simulate some search delay

        var items = new List<ISearchResultItem>
        {
            new SearchResultItem(
                Title: "Expense Tracker",
                Description: "Track your daily expenses",
                Url: "/finances/expense-tracker",
                Icon: "expense_icon",
                Color: "red"
            )
        };
        return new SearchResult(items.Count,
            items);
    }

    public void RegisterStorage(IStorageBuilder storageBuilder,
        IConfiguration configuration)
    {
        storageBuilder.RegisterStorage("RecipeImages");
    }
}
