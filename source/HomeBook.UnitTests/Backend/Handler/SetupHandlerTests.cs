using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Setup;
using HomeBook.Backend.Handler;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class SetupHandlerTests
{
    private ILogger<SetupHandler> _logger;
    private ISetupInstanceManager _setupInstanceManager = null!;
    private IFileService _fileService = null!;
    private ISetupConfigurationProvider _setupConfigurationProvider = null!;
    private IDatabaseManager _databaseManager = null!;

    [SetUp]
    public void SetUpSubstitutes()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Debug);
        });

        _logger = factory.CreateLogger<SetupHandler>();
        _setupInstanceManager = Substitute.For<ISetupInstanceManager>();
        _fileService = Substitute.For<IFileService>();
        _setupConfigurationProvider = Substitute.For<ISetupConfigurationProvider>();
        _databaseManager = Substitute.For<IDatabaseManager>();
    }

    [Test]
    public async Task HandleGetAvailability_returns_Conflict_when_instance_exists()
    {
        // Arrange
        _setupInstanceManager
            .IsSetupInstanceCreatedAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await SetupHandler.HandleGetAvailability(_logger, _setupInstanceManager, CancellationToken.None);

        // Assert
        var conflict = result.ShouldBeOfType<Conflict>();
        conflict.ShouldNotBeNull();
    }

    [Test]
    public async Task HandleGetAvailability_returns_Ok_when_instance_not_exists()
    {
        // Arrange
        _setupInstanceManager
            .IsSetupInstanceCreatedAsync(Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await SetupHandler.HandleGetAvailability(_logger, _setupInstanceManager, CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<Ok>();
        ok.ShouldNotBeNull();
    }

    [Test]
    public async Task HandleGetAvailability_returns_InternalServerError_on_exception()
    {
        // Arrange
        const string boom = "boom";
        _setupInstanceManager
            .IsSetupInstanceCreatedAsync(Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(boom));

        // Act
        var result = await SetupHandler.HandleGetAvailability(_logger, _setupInstanceManager, CancellationToken.None);

        // Assert
        var internalError = result.ShouldBeOfType<InternalServerError<string>>();
        internalError.Value.ShouldBe(boom);
    }

    [Test]
    public async Task HandleGetDatabaseCheck_returns_Ok_with_all_values()
    {
        // Arrange
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_HOST).Returns("db.example.local");
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PORT).Returns("3306");
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_NAME).Returns("homebook");
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_USER).Returns("hb_user");
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PASSWORD).Returns("s3cr3t");

        // Act
        var result = await SetupHandler.HandleGetDatabaseCheck(_logger, _fileService, _setupConfigurationProvider, CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<Ok<GetDatabaseCheckResponse>>();
        ok.Value.ShouldNotBeNull();
        ok.Value.DatabaseHost.ShouldBe("db.example.local");
        ok.Value.DatabasePort.ShouldBe("3306");
        ok.Value.DatabaseName.ShouldBe("homebook");
        ok.Value.DatabaseUserName.ShouldBe("hb_user");
        ok.Value.DatabaseUserPassword.ShouldBe("s3cr3t");

        // verify the provider was queried for each expected variable exactly once
        _setupConfigurationProvider.Received(1).GetValue(EnvironmentVariables.DATABASE_HOST);
        _setupConfigurationProvider.Received(1).GetValue(EnvironmentVariables.DATABASE_PORT);
        _setupConfigurationProvider.Received(1).GetValue(EnvironmentVariables.DATABASE_NAME);
        _setupConfigurationProvider.Received(1).GetValue(EnvironmentVariables.DATABASE_USER);
        _setupConfigurationProvider.Received(1).GetValue(EnvironmentVariables.DATABASE_PASSWORD);

        // file service is currently not used by the handler
        _fileService.DidNotReceiveWithAnyArgs();
    }

    [Test]
    public async Task HandleGetDatabaseCheck_returns_NotFound_ifNotAllIsFilledOut()
    {
        // Arrange: deliberately set some to null and some to empty
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_HOST).Returns((string?)null);
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PORT).Returns(string.Empty);
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_NAME).Returns((string?)null);
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_USER).Returns(" ");
        _setupConfigurationProvider.GetValue(EnvironmentVariables.DATABASE_PASSWORD).Returns((string?)null);

        // Act
        var result = await SetupHandler.HandleGetDatabaseCheck(_logger, _fileService, _setupConfigurationProvider, CancellationToken.None);

        // Assert: The current implementation always returns Ok with whatever it read
        var ok = result.ShouldBeOfType<NotFound>();
    }

    [Test]
    public async Task HandleGetDatabaseCheck_returns_InternalServerError_when_provider_throws()
    {
        // Arrange: throw on first var; could be any
        _setupConfigurationProvider
            .When(x => x.GetValue(EnvironmentVariables.DATABASE_HOST))
            .Do(_ => throw new InvalidOperationException("boom"));

        // Act
        var result = await SetupHandler.HandleGetDatabaseCheck(_logger, _fileService, _setupConfigurationProvider, CancellationToken.None);

        // Assert
        var internalErr = result.ShouldBeOfType<InternalServerError<string>>();
        internalErr.Value.ShouldBe("boom");
    }

    [Test]
    public async Task HandleCheckDatabase_WithDatabaseIsAvailable_ReturnsHttp200Ok()
    {
        // Arrange
        var databaseHost = "db.example.local";
        var databasePort = (ushort)3306;
        var databaseName = "homebook";
        var databaseUserName = "hb_user";
        var databaseUserPassword = "s3cr3t";
        _databaseManager.IsDatabaseAvailableAsync(databaseHost,
                databasePort,
                databaseName,
                databaseUserName,
                databaseUserPassword,
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var request = new CheckDatabaseRequest(databaseHost, databasePort, databaseName, databaseUserName, databaseUserPassword);
        var result = await SetupHandler.HandleCheckDatabase(request, _logger, _databaseManager, CancellationToken.None);

        // Assert
        var internalErr = result.ShouldBeOfType<Ok>();
    }

    [Test]
    public async Task HandleCheckDatabase_WithDatabaseIsNotAvailable_ReturnsHttp503ServiceUnavailable()
    {
        // Arrange
        var databaseHost = "db.example.local";
        var databasePort = (ushort)3306;
        var databaseName = "homebook";
        var databaseUserName = "hb_user";
        var databaseUserPassword = "s3cr3t";
        _databaseManager.IsDatabaseAvailableAsync(databaseHost,
                databasePort,
                databaseName,
                databaseUserName,
                databaseUserPassword,
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var request = new CheckDatabaseRequest(databaseHost, databasePort, databaseName, databaseUserName, databaseUserPassword);
        var result = await SetupHandler.HandleCheckDatabase(request, _logger, _databaseManager, CancellationToken.None);

        // Assert
        var internalErr = result.ShouldBeOfType<StatusCodeHttpResult>();
        internalErr.StatusCode.ShouldBe(503); // Service Unavailable
    }

    [Test]
    public async Task HandleCheckDatabase_Throws_ReturnsHttp500InternalServerError()
    {
        // Arrange
        var databaseHost = "db.example.local";
        var databasePort = (ushort)3306;
        var databaseName = "homebook";
        var databaseUserName = "hb_user";
        var databaseUserPassword = "s3cr3t";
        _databaseManager
            .When(x => x.IsDatabaseAvailableAsync(databaseHost,
                databasePort,
                databaseName,
                databaseUserName,
                databaseUserPassword,
                Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("boom"));

        // Act
        var request = new CheckDatabaseRequest(databaseHost, databasePort, databaseName, databaseUserName, databaseUserPassword);
        var result = await SetupHandler.HandleCheckDatabase(request, _logger, _databaseManager, CancellationToken.None);

        // Assert
        var internalErr = result.ShouldBeOfType<InternalServerError<string>>();
        internalErr.Value.ShouldBe("boom");
    }
}
