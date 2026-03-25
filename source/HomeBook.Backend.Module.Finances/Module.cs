using FluentValidation;
using HomeBook.Backend.Core.Finances.Validators;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Module.Finances.Contracts;
using HomeBook.Backend.Module.Finances.Endpoints;
using HomeBook.Backend.Module.Finances.Provider;
using HomeBook.Backend.Module.Finances.Services;
using HomeBook.Backend.Modules.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Module.Finances;

public class Module : IModule,
    IBackendModuleEndpointRegistrar,
    IBackendModuleServiceRegistrar,
    IBackendModuleSearchRegistrar
{
    public string Name { get; } = "Finances Module";
    public string Description { get; } = "Provides financial management features";
    public string Key { get; } = "homebook.finances";
    public string Author { get; } = "HomeBook";
    public Version Version { get; } = new("1.0.0");

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public void RegisterEndpoints(IEndpointBuilder builder, IConfiguration configuration)
    {
        builder.MapCalculationEndpoints()
            .MapSavingGoalEndpoints();
    }

    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISavingGoalsProvider, SavingGoalsProvider>();
        services.AddScoped<IFinanceCalculationsService, FinanceCalculationsService>();

        services.AddSingleton<IValidator<SavingGoal>, SavingGoalValidator>();
    }

    public static void RegisterSearch(ISearchBuilder searchBuilder,
        IConfiguration configuration)
    {
        searchBuilder.RegisterHandler<SearchHandler.SavingGoalSearchHandler>();
    }
}
