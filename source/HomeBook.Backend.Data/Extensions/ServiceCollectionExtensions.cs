using HomeBook.Backend.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendCoreData(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseManager, PostgreSqlDatabaseManager>();

        return services;
    }
}
