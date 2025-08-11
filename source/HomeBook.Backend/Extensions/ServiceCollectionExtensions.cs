using HomeBook.Backend.Abstractions;
using Homebook.Backend.Core.Setup.Provider;
using HomeBook.Backend.Services;

namespace HomeBook.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register the file service
#if DEBUG
        services.AddSingleton<IFileService, DebugFileService>();
#else
        services.AddSingleton<IFileService, NativeFileService>();
#endif

        // Register other services as needed
        // services.AddSingleton<IOtherService, OtherServiceImplementation>();// Program.cs

        return services;
    }
}
