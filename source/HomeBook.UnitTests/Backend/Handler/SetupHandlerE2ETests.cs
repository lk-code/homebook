using System.Text;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.HashProvider;
using HomeBook.Backend.Core.Licenses;
using Homebook.Backend.Core.Setup;
using Homebook.Backend.Core.Setup.Factories;
using Homebook.Backend.Core.Setup.Provider;
using Homebook.Backend.Core.Setup.Validators;
using HomeBook.Backend.Factories;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Provider;
using HomeBook.Backend.Requests;
using HomeBook.UnitTests.TestCore;
using HomeBook.UnitTests.TestCore.Backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Data.Sqlite;
using HomeBook.Backend.Extensions;
using HomeBook.UnitTests.TestCore.Helper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class SetupHandlerE2ETests
{
    private ILoggerFactory _loggerFactory;
    private SqliteConnection _keepAlive = null!;

    [SetUp]
    public void SetUpSubstitutes()
    {
        // create logger
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
    public async Task HandleStartSetup_RunFullSetupWithUpdates_Returns()
    {
        // Arrange
        var request = new StartSetupRequest(true,
            "SQLITE",
            null,
            null,
            null,
            null,
            null,
            "testuser",
            "s3cr3t-p1ssw0rd",
            "Test Homebook",
            "de-DE",
            "~/homebook-test.db");

        // Create initial configuration with JSON content that can be updated at runtime
        var initialConfigJson = new Dictionary<string, string?>
        {
            ["Environment"] = "UnitTests",
            ["Database:UseInMemory"] = "true"
        };

        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(initialConfigJson);

        IConfigurationRoot configuration = builder.Build();
        var wallpaperProvider = Substitute.For<IWallpaperProvider>();
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(configuration)
            .AddSingleton(wallpaperProvider)
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("SQLITE")
            .AddBackendSetup(configuration, InstanceStatus.SETUP)
            .BuildServiceProvider();

        var applicationPathProvider = (IApplicationPathProvider)new TestFileService();
        var fileSystemService = (IFileSystemService)new TestFileService();
        var runtimeConfigurationProvider = new RuntimeConfigurationProvider(
            _loggerFactory.CreateLogger<RuntimeConfigurationProvider>(),
            applicationPathProvider,
            fileSystemService);
        var setupInstanceManager = new SetupInstanceManager(
            _loggerFactory.CreateLogger<SetupInstanceManager>(),
            configuration,
            fileSystemService,
            applicationPathProvider);
        var licenseProvider = new LicenseProvider(
            _loggerFactory.CreateLogger<LicenseProvider>(),
            configuration,
            fileSystemService,
            applicationPathProvider);
        var setupConfigurationProvider = new SetupConfigurationProvider(
            _loggerFactory.CreateLogger<SetupConfigurationProvider>(),
            new EnvironmentValidator());
        var hostApplicationLifetime = new TestHostApplicationLifetime();
        var databaseMigratorFactory = new DatabaseMigratorFactory(serviceProvider,
            _loggerFactory.CreateLogger<DatabaseMigratorFactory>());
        var hashProviderFactory = new HashProviderFactory();
        var setupProcessorFactory = new SetupProcessorFactory(
            databaseMigratorFactory,
            hashProviderFactory,
            _loggerFactory,
            configuration,
            fileSystemService,
            applicationPathProvider,
            runtimeConfigurationProvider);

        ((fileSystemService as TestFileService)!).FileChanged += (sender, args) =>
        {
            string fileName = Path.GetFileName(args.FilePath);
            switch (fileName.ToLowerInvariant())
            {
                case "homebook.appsettings.json":
                {
                    // Parse the updated JSON content and rebuild configuration
                    try
                    {
                        var jsonContentBytes = args.Content;
                        if (jsonContentBytes is null)
                            break;

                        var jsonContent = Encoding.UTF8.GetString(jsonContentBytes);
                        if (!string.IsNullOrEmpty(jsonContent))
                        {
                            // Parse JSON to dictionary
                            var jsonDocument = JsonDocument.Parse(jsonContent);
                            var flattenedConfig = FlattenJsonElement(jsonDocument.RootElement);

                            // Rebuild configuration with updated values
                            var newBuilder = new ConfigurationBuilder()
                                .AddInMemoryCollection(flattenedConfig);

                            // Replace the current configuration's values
                            var newConfig = newBuilder.Build();
                            foreach (var kvp in flattenedConfig)
                            {
                                configuration[kvp.Key] = kvp.Value;
                            }

                            // Force reload to notify all configuration consumers
                            // configuration.Reload();
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Log JSON parsing error but don't fail the test
                        Console.WriteLine($"Failed to parse updated configuration JSON: {ex.Message}");
                    }
                }
                    break;
            }
        };

        // Act
        var result = await SetupHandler.HandleStartSetup(request,
            _loggerFactory.CreateLogger<SetupHandler>(),
            new SetupConfigurationValidator(),
            runtimeConfigurationProvider,
            setupInstanceManager,
            licenseProvider,
            setupConfigurationProvider,
            configuration,
            hostApplicationLifetime,
            setupProcessorFactory,
            CancellationToken.None);

        // Assert
        ((fileSystemService as TestFileService)!).FileChanged -= null;
        var response = result.ShouldBeOfType<Ok>();
        response.ShouldNotBeNull();

        // Verify that configuration was updated
        var tables = SqliteHelper.GetAllTableNames(_keepAlive)
            .Split(Environment.NewLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        tables.ShouldContain("__EFMigrationsHistory");
        tables.ShouldContain("Users");
        tables.ShouldContain("Configurations");

        // users
        var userDbEntries = SqliteHelper.DumpTable(_keepAlive, "Users");
        userDbEntries.Count.ShouldBe(2); // header + 1 user

        // configurations
        var configurationDbEntries = SqliteHelper.DumpTable(_keepAlive, "Configurations");
        configurationDbEntries.Count.ShouldBe(3); // header + 2 entries
        var expected = new string[,]
        {
            {
                "HOMEBOOK_CONFIGURATION_NAME",
                "Test Homebook"
            },
            {
                "HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE",
                "de-DE"
            }
        };
        var actual = configurationDbEntries.Select(r => (r[1], r[2])).ToList();
        for (int i = 0; i < expected.GetLength(0); i++)
        {
            var first = expected[i, 0];
            var second = expected[i, 1];
            actual.ShouldContain((first, second));
        }
    }

    // Helper method to flatten JSON structure into configuration key-value pairs
    private static Dictionary<string, string?> FlattenJsonElement(JsonElement element, string prefix = "")
    {
        var result = new Dictionary<string, string?>();

        foreach (var property in element.EnumerateObject())
        {
            string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                // Recursively flatten nested objects
                var nested = FlattenJsonElement(property.Value, key);
                foreach (var kvp in nested)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                // Convert value to string
                result[key] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => null,
                    _ => property.Value.GetRawText()
                };
            }
        }

        return result;
    }
}
