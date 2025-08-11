using FluentValidation;
using HomeBook.Backend.Abstractions;
using Homebook.Backend.Core.Setup.Models;
using Homebook.Backend.Core.Setup.Provider;
using Homebook.Backend.Core.Setup.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Homebook.Backend.Core.Setup.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendCoreSetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        //
        services.AddSetupEnvironment(configuration);

        services.AddSingleton<ISetupConfigurationProvider, SetupConfigurationProvider>();

        return services;
    }

    private static IServiceCollection AddSetupEnvironment(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IValidator<EnvironmentConfiguration>, EnvironmentValidator>();

        return services;
    }
}
