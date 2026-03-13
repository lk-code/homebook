using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBook.Backend.Data.Sqlite.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendDataSqliteProbe(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseProbe, SqliteProbe>();

        return services;
    }

    public static IServiceCollection AddBackendDataSqlite(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseManager, DatabaseManager>();

        // Initialize Database
        services.AddDbContextPool<AppDbContext>((sp, optionsBuilder) =>
            CreateDbContextOptionsBuilder(configuration, sp, optionsBuilder));

        services.AddDbContextFactory<AppDbContext>((sp, optionsBuilder) =>
            CreateDbContextOptionsBuilder(configuration, sp, optionsBuilder));

        return services;
    }

    public static void CreateDbContextOptionsBuilder(IConfiguration configuration,
        IServiceProvider sp,
        DbContextOptionsBuilder optionsBuilder)
    {
        bool useInMemory = configuration["Database:UseInMemory"] == "true";
        string connectionString = string.Empty;
        if (useInMemory)
            connectionString = ConnectionStringBuilder.BuildInMemory();
        else
            connectionString = ConnectionStringBuilder.Build(configuration["Database:File"]!);

        optionsBuilder.SetDbOptions(connectionString);

        IEnumerable<SaveChangesInterceptor> saveChangesInterceptors = sp.GetServices<SaveChangesInterceptor>();
        optionsBuilder.AddInterceptors(saveChangesInterceptors);
    }
}
