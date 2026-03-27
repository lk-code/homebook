using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Core.DataProvider.Extensions;
using HomeBook.Backend.Core.Extensions;
using Homebook.Backend.Core.Setup.Exceptions;
using Homebook.Backend.Core.Setup.Extensions;
using HomeBook.Backend.Data.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup;

/// <inheritdoc />
public class SetupProcessor(
    IDatabaseMigratorFactory databaseMigratorFactory,
    IHashProviderFactory hashProviderFactory,
    ILoggerFactory loggerFactory,
    IConfiguration injectedConfiguration,
    IFileSystemService fileSystemService,
    IApplicationPathProvider applicationPathProvider,
    IRuntimeConfigurationProvider runtimeConfigurationProvider) : ISetupProcessor
{
    /// <inheritdoc />
    public async Task ProcessAsync(IConfiguration configuration,
        SetupConfiguration setupConfiguration,
        CancellationToken cancellationToken = default)
    {
        // load specific database type provider
        string? databaseType = configuration["Database:Provider"];
        if (string.IsNullOrEmpty(databaseType))
            throw new SetupException("database provider is not configured");

        IDatabaseMigrator databaseMigrator = databaseMigratorFactory.CreateMigrator(databaseType);

        // 1. create database structure via migrations
        await databaseMigrator.MigrateAsync(cancellationToken);

        // 2. create user
        string? adminUsername = setupConfiguration.HomebookUserName;
        string? adminPassword = setupConfiguration.HomebookUserPassword;
        if (string.IsNullOrEmpty(adminUsername)
            || string.IsNullOrEmpty(adminPassword))
            throw new SetupException("homebook username or password is not set");

        // Get the database context using the new method with logging and configuration
        ServiceCollection services = new();

        // Add the same logging configuration as in Program.cs
        services.AddSingleton(loggerFactory);
        services.AddLogging();

        // Add the same configuration as in Program.cs
        services.AddSingleton(injectedConfiguration);

        // TODO: add IRuntimeConfigurationProvider

        // Add the same file system services as in Program.cs
        services.AddSingleton(fileSystemService);
        services.AddSingleton(applicationPathProvider);
        services.AddSingleton(hashProviderFactory);
        services.AddSingleton(databaseMigratorFactory);
        services.AddSingleton(runtimeConfigurationProvider);

        databaseMigrator.ConfigureForServiceCollection(services, configuration);

        services.AddSingleton<ISetupInstanceManager, SetupInstanceManager>();
        services.AddBackendCoreSetupUpdateComponents(configuration, InstanceStatus.SETUP)
            .AddBackendCore(configuration, InstanceStatus.SETUP)
            .AddBackendData(configuration, InstanceStatus.SETUP)
            .AddBackendCoreDataProvider(configuration, InstanceStatus.SETUP)
            .AddBackendDataValidators(configuration, InstanceStatus.SETUP);

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IUserProvider userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Guid adminUserId = await userProvider.CreateUserAsync(adminUsername,
            adminPassword,
            cancellationToken);

        IInstanceConfigurationProvider instanceConfigurationProvider =
            serviceProvider.GetRequiredService<IInstanceConfigurationProvider>();

        // 3. write homebook configuration
        string? configurationName = setupConfiguration.HomebookConfigurationName;
        if (string.IsNullOrEmpty(configurationName))
            throw new SetupException("homebook configuration name is not set");

        await instanceConfigurationProvider.SetHomeBookInstanceNameAsync(configurationName, cancellationToken);

        // 4. set default locale
        string? defaultLocale = setupConfiguration.HomebookConfigurationDefaultLocale;
        if (string.IsNullOrEmpty(defaultLocale))
            throw new SetupException("homebook default locale is not set");

        await instanceConfigurationProvider.SetHomeBookInstanceDefaultLocaleAsync(defaultLocale, cancellationToken);

        // 5. set wallpaper for admin user
        // TODO: set the wallpaper for admin user
        IUserPreferenceProvider userPreferenceProvider = serviceProvider.GetRequiredService<IUserPreferenceProvider>();
        string defaultWallpaperConfiguration = "stawp-Mountains.Light";
        await userPreferenceProvider.SetUserWallpaperAsync(adminUserId,
            defaultWallpaperConfiguration,
            cancellationToken);

        // 6. execute available updates
        IUpdateProcessor updateProcessor = serviceProvider.GetRequiredService<IUpdateProcessor>();
        await updateProcessor.ProcessAsync(cancellationToken);
    }
}
