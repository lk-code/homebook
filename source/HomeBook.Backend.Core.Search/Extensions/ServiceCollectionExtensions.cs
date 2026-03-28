using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Core.Search.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendCoreSearch(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddScoped<ISearchProvider, SearchProvider>();

        return services;
    }
}
