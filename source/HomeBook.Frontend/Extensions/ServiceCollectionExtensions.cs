using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Setup;

namespace HomeBook.Frontend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFrontendUiServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ISetupService, SetupService>();

        return services;
    }
}
