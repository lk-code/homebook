using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Sqlite;
using HomeBook.Backend.Data.Sqlite.Extensions;
using HomeBook.Backend.Extensions;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Responses;
using HomeBook.UnitTests.TestCore.Helper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class InfoHandlerE2ETests
{
    private ILoggerFactory _loggerFactory;
    private SqliteConnection _keepAlive = null!;

    [SetUp]
    public void SetUpSubstitutes()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug);
        });

        // create sqlite in memory
        var connectionString = ConnectionStringBuilder.BuildInMemory();

        _keepAlive = new SqliteConnection(connectionString);
        _keepAlive.Open();
    }

    [TearDown]
    public void TearDown()
    {
        // delete sqlite in memory
        _keepAlive.Close();

        // dispose logger
        _loggerFactory.Dispose();
    }

    [Test]
    public async Task RunFullLifecycle_Returns()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        IConfigurationRoot configuration = ConfigurationHelper.CreateConfigurationRoot(new Dictionary<string, string>
        {
            ["Environment"] = "UnitTests",
            ["Database:UseInMemory"] = "true",
            ["Database:Provider"] = "SQLITE"
        });
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(configuration)
            .AddBackendDataSqlite(configuration)
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("SQLITE")
            .AddDependenciesForRuntime(configuration, InstanceStatus.RUNNING)
            .BuildServiceProvider();
        // apply migrations
        var databaseMigrator = serviceProvider.GetKeyedService<IDatabaseMigrator>("SQLITE");
        await databaseMigrator.MigrateAsync(cancellationToken);

        // write test data
        IInstanceConfigurationProvider instanceConfigurationProvider = serviceProvider.GetRequiredService<IInstanceConfigurationProvider>();
        await instanceConfigurationProvider.SetHomeBookInstanceDefaultLocaleAsync("te-ST", cancellationToken);
        await instanceConfigurationProvider.SetHomeBookInstanceNameAsync("Test Instance", cancellationToken);


        // Act & Assert
        var instanceInfoResult = await InfoHandler.HandleGetInstanceInfo(
            _loggerFactory.CreateLogger<InfoHandler>(),
            instanceConfigurationProvider,
            cancellationToken);
        var instanceInfoResponse = instanceInfoResult.ShouldBeOfType<Ok<GetInstanceInfoResponse>>();
        instanceInfoResponse.Value.ShouldNotBeNull();
        instanceInfoResponse.Value.Name.ShouldBe("Test Instance");
        instanceInfoResponse.Value.DefaultLocale.ShouldBe("te-ST");

        var instanceNameResult = await InfoHandler.HandleGetInstanceName(
            _loggerFactory.CreateLogger<InfoHandler>(),
            instanceConfigurationProvider,
            cancellationToken);
        var instanceNameResponse = instanceNameResult.ShouldBeOfType<Ok<string>>();
        instanceNameResponse.Value.ShouldNotBeNull();
        instanceNameResponse.Value.ShouldBe("Test Instance");

        var instanceDefaultLocaleResult = await InfoHandler.HandleGetInstanceDefaultLocale(
            _loggerFactory.CreateLogger<InfoHandler>(),
            instanceConfigurationProvider,
            cancellationToken);
        var instanceDefaultLocaleResponse = instanceDefaultLocaleResult.ShouldBeOfType<Ok<string>>();
        instanceDefaultLocaleResponse.Value.ShouldNotBeNull();
        instanceDefaultLocaleResponse.Value.ShouldBe("te-ST");
    }
}
