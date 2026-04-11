using HomeBook.Client;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Modules.Abstractions;
using HomeBook.Frontend.Services.Provider;
using HomeBook.Frontend.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace HomeBook.Frontend.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontendServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IContentProvider, ContentProvider>();

        services.AddSingleton<ISystemManagementProvider, SystemManagementProvider>();
        services.AddSingleton<IUserManagementProvider, UserManagementProvider>();
        services.AddSingleton<IInstanceManagementProvider, InstanceManagementProvider>();
        services.AddSingleton<IUserPreferencesProvider, UserPreferencesProvider>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddSingleton<IJsLocalStorageProvider, JsLocalStorageProvider>();

        services.AddSingleton<IDatabaseSetupService, DatabaseSetupService>();
        services.AddSingleton<ILicensesService, LicensesService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<IWallpaperService, WallpaperService>();

        return services;
    }

    public static IServiceCollection AddBackendClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Replace with own auth provider which implements own jwt token if user is logged in
        services.AddSingleton<IAuthenticationProvider, AnonymousAuthenticationProvider>();

        services.AddSingleton<IRequestAdapter>(sp =>
        {
            string? backendHost = configuration["Backend:Host"];
            if (string.IsNullOrEmpty(backendHost))
            {
                throw new ArgumentNullException($"Backend:Host is not configured in appsettings.");
            }

            // if backendHost is a relative path, prepend the base URL
            if (!backendHost.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !backendHost.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                string? webAddress = configuration["Frontend:Host"];

                if (string.IsNullOrEmpty(webAddress))
                    throw new ArgumentNullException($"Frontend:Host is not configured");

                backendHost = $"{webAddress?.TrimEnd('/')}/{backendHost.TrimStart('/')}";
            }

            IAuthenticationProvider authProvider = sp.GetRequiredService<IAuthenticationProvider>();
            return new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient
            {
                BaseAddress = new Uri(backendHost)
            });
        });

        services.AddSingleton<BackendClient>(sp =>
        {
            IRequestAdapter adapter = sp.GetRequiredService<IRequestAdapter>();
            return new BackendClient(adapter);
        });

        return services;
    }
}
