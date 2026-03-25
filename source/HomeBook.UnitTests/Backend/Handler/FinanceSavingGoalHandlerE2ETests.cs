using System.Security.Claims;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Core.Search;
using HomeBook.Backend.Data.Sqlite;
using HomeBook.Backend.Data.Sqlite.Extensions;
using HomeBook.Backend.Extensions;
using HomeBook.Backend.Factories;
using HomeBook.Backend.Module.Finances.Contracts;
using HomeBook.Backend.Module.Finances.Enums;
using HomeBook.Backend.Module.Finances.Handler;
using HomeBook.Backend.Module.Finances.Requests;
using HomeBook.Backend.Module.Finances.Responses;
using HomeBook.UnitTests.TestCore.Backend;
using HomeBook.UnitTests.TestCore.Helper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class FinanceSavingGoalHandlerE2ETests
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
    public async Task RunFullLifecycle_Returns()
    {
        // Arrange
        CancellationToken cancellationToken = CancellationToken.None;
        string testUserName = "testuser";
        string testUserPassword = "s3cr3tP@ssw0rd!";

        // create configuration and service provider
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
            .AddDependenciesForRuntime(configuration, InstanceStatus.SETUP)
            .AddBackendModulesForTestEnvironment(configuration)
            .BuildServiceProvider();

        // apply migrations
        var databaseMigrator = serviceProvider.GetKeyedService<IDatabaseMigrator>("SQLITE")!;
        await databaseMigrator.MigrateAsync(cancellationToken);

        // verify that migrations were applied
        var tables = SqliteHelper.GetAllTableNames(_keepAlive)
            .Split(Environment.NewLine)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        tables.ShouldContain("__EFMigrationsHistory");
        tables.ShouldContain("Users");
        tables.ShouldContain("Configurations");
        tables.ShouldContain("UserPreferences");
        tables.ShouldContain("SavingGoals");

        // create user
        IUserProvider userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        Guid testUserId = await userProvider.CreateUserAsync(testUserName,
            testUserPassword,
            cancellationToken);
        ClaimsPrincipal testuser = UserHelper.CreateTestUser(testUserId,
            testUserName);

        // create instances for tests
        ISavingGoalsProvider savingGoalsProvider = serviceProvider.GetRequiredService<ISavingGoalsProvider>();

        // Act & Assert

        // create saving goal
        var createRequest = new CreateSavingGoalRequest("Test Saving Goal",
            "#ff0000",
            "icon-svg",
            12_000,
            8_500,
            500,
            (int)InterestRateOptions.MONTHLY,
            0,
            null);
        var createSavingGoalResult = await SavingGoalHandler.HandleCreateSavingGoal(testuser,
            createRequest,
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        createSavingGoalResult.ShouldBeOfType<Ok>();

        // get all after creation
        var savingGoalsResult1 = await SavingGoalHandler.HandleGetSavingGoals(testuser,
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        var savingGoalsResponse1 = savingGoalsResult1.ShouldBeOfType<Ok<SavingGoalListResponse>>();
        savingGoalsResponse1.Value.ShouldNotBeNull();
        savingGoalsResponse1.Value.SavingGoals.Length.ShouldBe(1);
        Guid createdSavingGoalId = savingGoalsResponse1.Value.SavingGoals[0].Id;
        savingGoalsResponse1.Value.SavingGoals[0].Color.ShouldBe("#ff0000");
        savingGoalsResponse1.Value.SavingGoals[0].Name.ShouldBe("Test Saving Goal");
        savingGoalsResponse1.Value.SavingGoals[0].TargetAmount.ShouldBe(12_000);
        savingGoalsResponse1.Value.SavingGoals[0].CurrentAmount.ShouldBe(8_500);
        savingGoalsResponse1.Value.SavingGoals[0].MonthlyPayment.ShouldBe(500);

        // update saving goal
        var updateSavingGoalNameResult = await SavingGoalHandler.HandleUpdateSavingGoalName(createdSavingGoalId,
            testuser,
            new UpdateSavingGoalNameRequest("Updated Saving Goal"),
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        updateSavingGoalNameResult.ShouldBeOfType<Ok>();

        // update saving goal current amount
        var updateSavingGoalAmountsResult = await SavingGoalHandler.HandleUpdateSavingGoalAmounts(
            createdSavingGoalId,
            testuser,
            new UpdateSavingGoalAmountsRequest(null,
                9_000,
                null,
                null,
                null),
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        updateSavingGoalAmountsResult.ShouldBeOfType<Ok>();

        // update saving goal current amount
        var dt = DateTime.UtcNow.AddYears(3);
        var updateSavingGoalInfoResult = await SavingGoalHandler.HandleUpdateSavingGoalInfo(
            createdSavingGoalId,
            testuser,
            new UpdateSavingGoalInfoRequest(new DateTime(dt.Year, dt.Month, dt.Day)),
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        updateSavingGoalInfoResult.ShouldBeOfType<Ok>();

        // update saving goal current amount
        var updateSavingGoalAppearanceResult = await SavingGoalHandler.HandleUpdateSavingGoalAppearance(
            createdSavingGoalId,
            testuser,
            new UpdateSavingGoalAppearanceRequest("#0000ff", ""),
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        updateSavingGoalAppearanceResult.ShouldBeOfType<Ok>();

        // get all after update
        var savingGoalsResult2 = await SavingGoalHandler.HandleGetSavingGoals(testuser,
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        var savingGoalsResponse2 = savingGoalsResult2.ShouldBeOfType<Ok<SavingGoalListResponse>>();
        savingGoalsResponse2.Value.ShouldNotBeNull();
        savingGoalsResponse2.Value.SavingGoals.Length.ShouldBe(1);
        savingGoalsResponse2.Value.SavingGoals[0].Color.ShouldBe("#0000ff");
        savingGoalsResponse2.Value.SavingGoals[0].Name.ShouldBe("Updated Saving Goal");
        savingGoalsResponse2.Value.SavingGoals[0].TargetAmount.ShouldBe(12_000);
        savingGoalsResponse2.Value.SavingGoals[0].CurrentAmount.ShouldBe(9_000);
        savingGoalsResponse2.Value.SavingGoals[0].MonthlyPayment.ShouldBe(500);
        savingGoalsResponse2.Value.SavingGoals[0].TargetDate.ShouldBe(new DateTime(dt.Year, dt.Month, dt.Day));

        // delete saving goal
        var deleteSavingGoalResult = await SavingGoalHandler.HandleDeleteSavingGoal(createdSavingGoalId,
            testuser,
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        deleteSavingGoalResult.ShouldBeOfType<Ok>();

        // get all after delete
        var savingGoalsResult3 = await SavingGoalHandler.HandleGetSavingGoals(testuser,
            _loggerFactory.CreateLogger<SavingGoalHandler>(),
            savingGoalsProvider,
            cancellationToken);
        var savingGoalsResponse3 = savingGoalsResult3.ShouldBeOfType<Ok<SavingGoalListResponse>>();
        savingGoalsResponse3.Value.ShouldNotBeNull();
        savingGoalsResponse3.Value.SavingGoals.Length.ShouldBe(0);
    }
}
