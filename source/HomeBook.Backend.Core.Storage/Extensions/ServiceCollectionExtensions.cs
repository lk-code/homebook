using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Core.Storage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendCoreStorage(this IServiceCollection services,
        IConfiguration configuration,
        InstanceStatus instanceStatus)
    {
        services.AddScoped<IStorageProvider, StorageFileSystemProvider>();
        services.AddScoped<IMediaProvider, MediaProvider>();

        return services;
    }
}
