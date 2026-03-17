using FluentValidation;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Interceptors;
using HomeBook.Backend.Data.Repositories;
using HomeBook.Backend.Data.Validators;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendData(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<ISavingGoalsRepository, SavingGoalsRepository>();
        services.AddScoped<IRecipesRepository, RecipesRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IStorageScopeRegistrationRepository, StorageScopeRegistrationRepository>();
        services.AddScoped<IMediaItemRepository, MediaItemRepository>();

        services.AddSingleton<SaveChangesInterceptor, NormalizationInterceptor>();

        return services;
    }

    public static IServiceCollection AddBackendDataValidators(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddSingleton<IValidator<User>, UserValidator>();
        services.AddSingleton<IValidator<Configuration>, ConfigurationValidator>();
        services.AddSingleton<IValidator<UserPreference>, UserPreferenceValidator>();

        return services;
    }
}
