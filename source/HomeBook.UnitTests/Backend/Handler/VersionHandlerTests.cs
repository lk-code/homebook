using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using Shouldly;
using HomeBook.Backend.Handler;

namespace HomeBook.UnitTests.Backend.Handler;

[TestFixture]
public class VersionHandlerTests
{
    private IConfiguration _configuration;
    private CancellationToken _cancellationToken;

    [SetUp]
    public void SetUp()
    {
        _configuration = Substitute.For<IConfiguration>();
        _cancellationToken = CancellationToken.None;
    }

    [Test]
    public async Task HandleGetVersion_WithValidVersion_ShouldReturnOkWithVersion()
    {
        // Arrange
        const string expectedVersion = "1.2.3";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(expectedVersion);
    }

    [Test]
    public async Task HandleGetVersion_WithEmptyVersion_ShouldReturnInternalServerError()
    {
        // Arrange
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(string.Empty);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var errorResult = result.ShouldBeOfType<InternalServerError<string>>();
        errorResult.Value.ShouldBe("Service Version is not configured.");
    }

    [Test]
    public async Task HandleGetVersion_WithNullVersion_ShouldReturnInternalServerError()
    {
        // Arrange
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns((string?)null);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var errorResult = result.ShouldBeOfType<InternalServerError<string>>();
        errorResult.Value.ShouldBe("Service Version is not configured.");
    }

    [Test]
    public async Task HandleGetVersion_WithWhitespaceVersion_ShouldReturnInternalServerError()
    {
        // Arrange
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns("   ");
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var errorResult = result.ShouldBeOfType<InternalServerError<string>>();
        errorResult.Value.ShouldBe("Service Version is not configured.");
    }

    [Test]
    public async Task HandleGetVersion_WithVersionContainingSpecialCharacters_ShouldReturnOkWithVersion()
    {
        // Arrange
        const string expectedVersion = "1.0.0-beta.1+build.123";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(expectedVersion);
    }

    [Test]
    public async Task HandleGetVersion_WithLongVersionString_ShouldReturnOkWithVersion()
    {
        // Arrange
        const string expectedVersion = "1.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(expectedVersion);
    }

    [TestCase("1.0.0")]
    [TestCase("1.2.3")]
    [TestCase("10.20.30")]
    [TestCase("0.0.1")]
    [TestCase("999.999.999")]
    public async Task HandleGetVersion_WithValidVersionFormats_ShouldReturnOkWithVersion(string version)
    {
        // Arrange
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(version);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(version);
    }

    [Test]
    public async Task HandleGetVersion_WithCancelledToken_ShouldStillExecute()
    {
        // Arrange
        const string expectedVersion = "1.0.0";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);
        var cancelledToken = new CancellationToken(true);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, cancelledToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(expectedVersion);
    }

    [Test]
    public async Task HandleGetVersion_ShouldCallCorrectConfigurationSection()
    {
        // Arrange
        const string expectedVersion = "1.0.0";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        _configuration.Received(1).GetSection("Version");
    }

    [Test]
    public async Task HandleGetVersion_WithNullConfigurationSection_ShouldReturnInternalServerError()
    {
        // Arrange
        _configuration.GetSection("Version").Returns((IConfigurationSection?)null);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var errorResult = result.ShouldBeOfType<InternalServerError<string>>();
        errorResult.Value.ShouldBe("Service Version is not configured.");
    }

    [Test]
    public async Task HandleGetVersion_WithVersionAsNumber_ShouldReturnOkWithVersion()
    {
        // Arrange
        const string expectedVersion = "123";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(expectedVersion);
    }

    [Test]
    public async Task HandleGetVersion_WithVersionAsDate_ShouldReturnOkWithVersion()
    {
        // Arrange
        const string expectedVersion = "2023-12-25";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        // Act
        var result = await VersionHandler.HandleGetVersion(_configuration, _cancellationToken);

        // Assert
        var okResult = result.ShouldBeOfType<Ok<string>>();
        okResult.Value.ShouldBe(expectedVersion);
    }
}
