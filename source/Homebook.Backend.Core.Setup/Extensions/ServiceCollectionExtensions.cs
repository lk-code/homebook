using FluentValidation;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using Homebook.Backend.Core.Setup.Factories;
using Homebook.Backend.Core.Setup.Models;
using Homebook.Backend.Core.Setup.Provider;
using Homebook.Backend.Core.Setup.UpdateMigrators;
using Homebook.Backend.Core.Setup.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Homebook.Backend.Core.Setup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendCoreSetup(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddBackendCoreSetupEnvironment(configuration, instanceStatus)
            .AddBackendCoreSetupValidators(configuration, instanceStatus)
            .AddBackendCoreSetupUpdateComponents(configuration, instanceStatus);

        services.AddSingleton<ISetupConfigurationProvider, SetupConfigurationProvider>();
        services.AddSingleton<ISetupInstanceManager, SetupInstanceManager>();
        services.AddScoped<ISetupProcessorFactory, SetupProcessorFactory>();

        return services;
    }

    public static IServiceCollection AddBackendCoreSetupUpdateComponents(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddScoped<IUpdateManager, UpdateManager>();
        services.AddScoped<IUpdateProcessor, UpdateProcessor>();

        // update migrators
        services.AddScoped<IUpdateMigrator, Update_20250910_01>();
        services.AddScoped<IUpdateMigrator, Update_20250912_01>();
        services.AddScoped<IUpdateMigrator, Update_20250925_01>();
        services.AddScoped<IUpdateMigrator, Update_20250925_02>();
        services.AddScoped<IUpdateMigrator, Update_20260330_01>();

        return services;
    }

    private static IServiceCollection AddBackendCoreSetupEnvironment(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddSingleton<IValidator<EnvironmentConfiguration>, EnvironmentValidator>();

        return services;
    }

    private static IServiceCollection AddBackendCoreSetupValidators(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddSingleton<IValidator<SetupConfiguration>, SetupConfigurationValidator>();

        return services;
    }
}
