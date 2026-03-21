using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Exceptions;
using HomeBook.Backend.Data.Sqlite;
using HomeBook.Backend.Factories;
using HomeBook.UnitTests.TestCore.Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HomeBook.UnitTests.Backend.Factories;

[TestFixture]
public class DatabaseMigratorFactoryTests
{
    private IServiceProvider _serviceProvider;
    private DatabaseMigratorFactory _instance;

    [SetUp]
    public void SetUp()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Substitute.For<IDatabaseMigratorFactory>())
            .AddKeyedSingleton<IDatabaseMigrator, DatabaseMigrator>("UNITDB")
            .AddSingleton(Substitute.For<IConfiguration>())
            .BuildServiceProvider();

        _instance = new DatabaseMigratorFactory(_serviceProvider,
            NullLogger<DatabaseMigratorFactory>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [Test]
    public void CreateMigrator_L_ShouldReturnMigrator()
    {
        // Act
        var result = _instance.CreateMigrator("UNITDB");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<DatabaseMigrator>();
    }

    [Test]
    public void CreateMigrator_WithInvalidType_ShouldThrowUnsupportedDatabaseException()
    {
        // Act & Assert
        var exception = Should.Throw<UnsupportedDatabaseException>(() => _instance.CreateMigrator("INVALID"));
        exception.Message.ShouldContain("Unsupported database provider: INVALID");
    }
}
