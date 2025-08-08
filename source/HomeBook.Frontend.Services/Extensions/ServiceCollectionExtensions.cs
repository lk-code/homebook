using HomeBook.Client;
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
        // services.AddSingleton<ISetupService, SetupService>();

        return services;
    }

    public static IServiceCollection AddBackendClient(this IServiceCollection services,
        IConfiguration configuration)
    {
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

        services.AddScoped<BackendClient>(sp =>
        {
            IRequestAdapter adapter = sp.GetRequiredService<IRequestAdapter>();
            return new BackendClient(adapter);
        });

        return services;
    }
}
