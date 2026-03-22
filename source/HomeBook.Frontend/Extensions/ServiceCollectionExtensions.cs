using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.ModuleCore;
using HomeBook.Frontend.Modules.Abstractions;
using HomeBook.Frontend.Provider;
using HomeBook.Frontend.Services;
using HomeBook.Frontend.Setup;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.FeatureManagement;

namespace HomeBook.Frontend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontendUiServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(configuration)
            .AddLocalization()
            .AddFeatureManagement();

        services.AddSingleton<ISetupService, SetupService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddSingleton<IFileStorageRegistration, FileStorageRegistration>();
        services.AddSingleton<IMediaService, MediaService>();
        services.AddSingleton<ISystemStorageProvider, SystemStorageProvider>();
        services.AddSingleton<IDeveloperService, DeveloperService>();

        services.AddSingleton<IWidgetFactory, WidgetFactory>();

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        return services;
    }
}
