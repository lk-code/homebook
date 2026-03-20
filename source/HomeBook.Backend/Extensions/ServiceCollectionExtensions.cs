using FluentValidation;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Exceptions;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Core;
using HomeBook.Backend.Core.Account.Extensions;
using HomeBook.Backend.Core.DataProvider;
using HomeBook.Backend.Core.Extensions;
using HomeBook.Backend.Core.HashProvider;
using HomeBook.Backend.Core.Licenses;
using HomeBook.Backend.Core.Licenses.Extensions;
using HomeBook.Backend.Core.Search.Extensions;
using Homebook.Backend.Core.Setup;
using Homebook.Backend.Core.Setup.Extensions;
using Homebook.Backend.Core.Setup.Factories;
using Homebook.Backend.Core.Setup.Models;
using Homebook.Backend.Core.Setup.Provider;
using Homebook.Backend.Core.Setup.Validators;
using HomeBook.Backend.Core.Storage.Extensions;
using HomeBook.Backend.Data;
using HomeBook.Backend.Data.Extensions;
using HomeBook.Backend.Data.Mysql.Extensions;
using HomeBook.Backend.Data.PostgreSql.Extensions;
using HomeBook.Backend.Data.Sqlite.Extensions;
using HomeBook.Backend.Factories;
using HomeBook.Backend.Provider;
using HomeBook.Backend.Services;

namespace HomeBook.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDependenciesForSetup(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddBackendSetup(configuration, instanceStatus);

        return services;
    }

    public static IServiceCollection AddDependenciesForRuntime(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddBackendServices(configuration, instanceStatus)
            .AddBackendCore(configuration, instanceStatus)
            .AddBackendCoreSetup(configuration, instanceStatus)
            .AddBackendCoreLicenses(configuration, instanceStatus)
            .AddBackendCoreSearch(configuration, instanceStatus)
            .AddBackendCoreStorage(configuration, instanceStatus)
            .AddBackendDatabaseProvider(configuration, instanceStatus)
            .AddAccountServices(configuration, instanceStatus);

        return services;
    }

    /// <summary>
    /// add ALL services required for setup mode
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="instanceStatus"></param>
    /// <returns></returns>
    public static IServiceCollection AddBackendSetup(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        // validators
        services.AddBackendDataValidators(configuration,
            instanceStatus);
        services.AddSingleton<IValidator<SetupConfiguration>, SetupConfigurationValidator>();
        services.AddSingleton<IValidator<EnvironmentConfiguration>, EnvironmentValidator>();

        // basic dependencies
        services.AddSingleton<IFileSystemService, NativeFileService>();
        services.AddSingleton<IApplicationPathProvider, NativeFileService>();
        services.AddSingleton<IRuntimeConfigurationProvider, RuntimeConfigurationProvider>();
        services.AddSingleton<IHashProviderFactory, HashProviderFactory>();
        services.AddSingleton<ILicenseProvider, LicenseProvider>();
        services.AddSingleton<IStringNormalizer, StringNormalizer>();

        // setup dependencies
        services.AddSingleton<ISetupInstanceManager, SetupInstanceManager>();
        services.AddSingleton<ISetupConfigurationProvider, SetupConfigurationProvider>();
        services.AddScoped<ISetupProcessorFactory, SetupProcessorFactory>();

        // database dependencies
        services.AddBackendDatabaseProbes(configuration, instanceStatus);
        services.AddBackendDatabaseMigrators(configuration, instanceStatus);
        services.AddSingleton<IDatabaseMigratorFactory, DatabaseMigratorFactory>();

        return services;
    }

    public static IServiceCollection AddBackendServices(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        // Register the file service
        services.AddSingleton<ISystemStorageService, SystemStorageService>();
        services.AddSingleton<IApplicationPathProvider, NativeFileService>();
        services.AddSingleton<IFileSystemService, NativeFileService>();
        services.AddBackendDatabaseMigrators(configuration, instanceStatus);
        services.AddSingleton<IDatabaseMigratorFactory, DatabaseMigratorFactory>();

        // Register other services as needed
        services.AddSingleton<IRuntimeConfigurationProvider, RuntimeConfigurationProvider>();

        return services;
    }

    public static IServiceCollection AddBackendDatabaseMigrators(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddKeyedSingleton<IDatabaseMigrator, Data.PostgreSql.DatabaseMigrator>("POSTGRESQL");
        services.AddKeyedSingleton<IDatabaseMigrator, Data.Mysql.DatabaseMigrator>("MYSQL");
        services.AddKeyedSingleton<IDatabaseMigrator, Data.Sqlite.DatabaseMigrator>("SQLITE");

        return services;
    }

    public static IServiceCollection AddBackendDatabaseProbes(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddSingleton<IDatabaseProviderResolver, DatabaseProviderResolver>();
        services.AddBackendDataPostgreSqlProbe(configuration);
        services.AddBackendDataMysqlProbe(configuration);
        services.AddBackendDataSqliteProbe(configuration);

        return services;
    }

    public static IServiceCollection AddBackendDatabaseProvider(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddBackendDatabaseProbes(configuration, instanceStatus);

        // Get database provider from configuration
        string? databaseType = configuration["Database:Provider"];
        if (!string.IsNullOrEmpty((databaseType ?? string.Empty).Trim()))
        {
            // load database provider specific services
            switch (databaseType?.ToLowerInvariant())
            {
                case "postgresql":
                    services.AddBackendDataPostgreSql(configuration);
                    break;
                case "mysql":
                    services.AddBackendDataMysql(configuration);
                    break;
                case "sqlite":
                    services.AddBackendDataSqlite(configuration);
                    break;
                default:
                    throw new UnsupportedDatabaseException($"Unsupported database provider: {databaseType}");
            }

            // load common database services (repositories, etc.)
            services.AddBackendData(configuration, instanceStatus)
                .AddBackendCoreDataProvider(configuration, instanceStatus);
        }

        services.AddBackendDataValidators(configuration, instanceStatus);

        return services;
    }

    public static IServiceCollection AddBackendCoreDataProvider(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddScoped<IUserProvider, UserProvider>();
        services.AddScoped<IInstanceConfigurationProvider, InstanceConfigurationProvider>();
        services.AddScoped<IUserPreferenceProvider, UserPreferenceProvider>();

        return services;
    }
}
