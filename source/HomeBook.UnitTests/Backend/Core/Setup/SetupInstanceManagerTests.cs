using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Enums;
using HomeBook.Backend.Abstractions.Models;
using Homebook.Backend.Core.Setup;
using HomeBook.UnitTests.TestCore.Backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HomeBook.UnitTests.Backend.Core.Setup;

[TestFixture]
public class SetupInstanceManagerTests
{
    private CancellationToken _cancellationToken;
    private ILogger<SetupInstanceManager> _logger;
    private IConfiguration _configuration;
    private IFileSystemService _fileSystemService;
    private IApplicationPathProvider _applicationPathProvider;
    private SetupInstanceManager _instance;

    [SetUp]
    public void SetUp()
    {
        _cancellationToken = CancellationToken.None;
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
        _logger = factory.CreateLogger<SetupInstanceManager>();
        _configuration = Substitute.For<IConfiguration>();
        _fileSystemService = Substitute.For<IFileSystemService>();
        _applicationPathProvider = Substitute.For<IApplicationPathProvider>();
        _instance = new SetupInstanceManager(_logger, _configuration, _fileSystemService, _applicationPathProvider);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithNullVersions_ReturnNoUpdate()
    {
        // Arrange
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns((string)null!);
        _configuration.GetSection("Version").Returns(configSection);

        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns((string)null!);

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithAppVersionAndNullInstanceVersion_ReturnNoUpdate()
    {
        // Arrange
        const string expectedVersion = "1.2.3";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns((string)null!);

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithNullAppVersionAndInstanceVersion_ReturnNoUpdate()
    {
        // Arrange
        const string expectedVersion = "1.2.3";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns((string)null!);
        _configuration.GetSection("Version").Returns(configSection);

        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedVersion);

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithSameVersions_ReturnNoUpdate()
    {
        // Arrange
        const string expectedVersion = "1.2.3";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(expectedVersion);
        _configuration.GetSection("Version").Returns(configSection);

        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedVersion);

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithLowerAppVersion_ReturnNoUpdate()
    {
        // Arrange
        const string appVersion = "1.0.0";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(appVersion);
        _configuration.GetSection("Version").Returns(configSection);

        const string instanceVersion = "1.1.0";
        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(instanceVersion);

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithHigherAppVersion_ReturnUpdateAvailable()
    {
        // Arrange
        const string appVersion = "1.1.0";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(appVersion);
        _configuration.GetSection("Version").Returns(configSection);

        const string instanceVersion = "1.0.0";
        _fileSystemService.FileExists(Arg.Any<string>())
            .Returns(true);
        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(instanceVersion);

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(true);
    }

    [Test]
    public async Task IsUpdateRequiredAsync_WithFileReadException_ReturnNoUpdate()
    {
        // Arrange
        const string appVersion = "1.1.0";
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(appVersion);
        _configuration.GetSection("Version").Returns(configSection);

        _fileSystemService.FileReadAllTextAsync(Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Throws(new FileNotFoundException("Instance file not found"));

        // Act
        var result = await _instance.IsUpdateRequiredAsync(_cancellationToken);

        // Assert
        result.ShouldBe(false);
    }

    // [Test]
    // public void IsSetupFinishedAsync_WithExistingInstanceFile_ReturnsTrue()
    // {
    //     // Arrange
    //     _fileSystemService.FileExists(Arg.Any<string>()).Returns(true);
    //
    //     // Act
    //     var result = _instance.IsSetupFinishedAsync(_cancellationToken);
    //
    //     // Assert
    //     result.ShouldBe(true);
    // }

    // [Test]
    // public void IsSetupFinishedAsync_WithNonExistingInstanceFile_ReturnsFalse()
    // {
    //     // Arrange
    //     _fileSystemService.FileExists(Arg.Any<string>()).Returns(false);
    //
    //     // Act
    //     var result = _instance.IsSetupFinishedAsync(_cancellationToken);
    //
    //     // Assert
    //     result.ShouldBe(false);
    // }
    //
    // [Test]
    // public void IsSetupFinishedAsync_CallsFileExistsWithCorrectPath()
    // {
    //     // Arrange
    //     const string configPath = "/test/config";
    //     const string expectedFilePath = "/test/config/.homebook";
    //     _applicationPathProvider.ConfigurationPath.Returns(configPath);
    //     _fileSystemService.FileExists(Arg.Any<string>()).Returns(true);
    //
    //     // Create new instance to test with mocked path
    //     var instance = new SetupInstanceManager(_logger, _configuration, _fileSystemService, _applicationPathProvider);
    //
    //     // Act
    //     instance.IsSetupFinishedAsync(_cancellationToken);
    //
    //     // Assert
    //     _fileSystemService.Received(1).FileExists(expectedFilePath);
    // }

    [Test]
    public void CreateRequiredDirectories_WithAllDirectoriesExisting_DoesNotCreateAny()
    {
        // Arrange
        _applicationPathProvider.ConfigurationPath.Returns("/config");
        _applicationPathProvider.CacheDirectory.Returns("/cache");
        _applicationPathProvider.LogDirectory.Returns("/logs");
        _applicationPathProvider.DataDirectory.Returns("/data");
        _applicationPathProvider.TempDirectory.Returns("/temp");

        _fileSystemService.DirectoryExists(Arg.Any<string>()).Returns(true);

        // Act
        _instance.CreateRequiredDirectories();

        // Assert
        _fileSystemService.DidNotReceive().CreateDirectory(Arg.Any<string>());
    }

    [Test]
    public void CreateRequiredDirectories_WithNoDirectoriesExisting_CreatesAllDirectories()
    {
        // Arrange
        const string configPath = "/config";
        const string cachePath = "/cache";
        const string logPath = "/logs";
        const string dataPath = "/data";
        const string tempPath = "/temp";

        _applicationPathProvider.ConfigurationPath.Returns(configPath);
        _applicationPathProvider.CacheDirectory.Returns(cachePath);
        _applicationPathProvider.LogDirectory.Returns(logPath);
        _applicationPathProvider.DataDirectory.Returns(dataPath);
        _applicationPathProvider.TempDirectory.Returns(tempPath);

        _fileSystemService.DirectoryExists(Arg.Any<string>()).Returns(false);

        // Act
        _instance.CreateRequiredDirectories();

        // Assert
        _fileSystemService.Received(1).CreateDirectory(configPath);
        _fileSystemService.Received(1).CreateDirectory(cachePath);
        _fileSystemService.Received(1).CreateDirectory(logPath);
        _fileSystemService.Received(1).CreateDirectory(dataPath);
        _fileSystemService.Received(1).CreateDirectory(tempPath);
    }

    [Test]
    public void CreateRequiredDirectories_WithSomeDirectoriesExisting_CreatesOnlyMissingDirectories()
    {
        // Arrange
        const string configPath = "/config";
        const string cachePath = "/cache";
        const string logPath = "/logs";
        const string dataPath = "/data";
        const string tempPath = "/temp";

        _applicationPathProvider.ConfigurationPath.Returns(configPath);
        _applicationPathProvider.CacheDirectory.Returns(cachePath);
        _applicationPathProvider.LogDirectory.Returns(logPath);
        _applicationPathProvider.DataDirectory.Returns(dataPath);
        _applicationPathProvider.TempDirectory.Returns(tempPath);

        // Configure existing directories
        _fileSystemService.DirectoryExists(configPath).Returns(true);
        _fileSystemService.DirectoryExists(cachePath).Returns(false);
        _fileSystemService.DirectoryExists(logPath).Returns(true);
        _fileSystemService.DirectoryExists(dataPath).Returns(false);
        _fileSystemService.DirectoryExists(tempPath).Returns(true);

        // Act
        _instance.CreateRequiredDirectories();

        // Assert
        _fileSystemService.DidNotReceive().CreateDirectory(configPath);
        _fileSystemService.Received(1).CreateDirectory(cachePath);
        _fileSystemService.DidNotReceive().CreateDirectory(logPath);
        _fileSystemService.Received(1).CreateDirectory(dataPath);
        _fileSystemService.DidNotReceive().CreateDirectory(tempPath);
    }

    [Test]
    public void CreateRequiredDirectories_ChecksAllRequiredDirectories()
    {
        // Arrange
        const string configPath = "/config";
        const string cachePath = "/cache";
        const string logPath = "/logs";
        const string dataPath = "/data";
        const string tempPath = "/temp";

        _applicationPathProvider.ConfigurationPath.Returns(configPath);
        _applicationPathProvider.CacheDirectory.Returns(cachePath);
        _applicationPathProvider.LogDirectory.Returns(logPath);
        _applicationPathProvider.DataDirectory.Returns(dataPath);
        _applicationPathProvider.TempDirectory.Returns(tempPath);

        _fileSystemService.DirectoryExists(Arg.Any<string>()).Returns(true);

        // Act
        _instance.CreateRequiredDirectories();

        // Assert
        _fileSystemService.Received(1).DirectoryExists(configPath);
        _fileSystemService.Received(1).DirectoryExists(cachePath);
        _fileSystemService.Received(1).DirectoryExists(logPath);
        _fileSystemService.Received(1).DirectoryExists(dataPath);
        _fileSystemService.Received(1).DirectoryExists(tempPath);
    }

    [Test]
    public void IsSetupInstanceCreated_WithExistingSetupFile_ReturnsTrue()
    {
        // Arrange
        _fileSystemService.FileExists(Arg.Any<string>()).Returns(true);

        // Act
        var result = _instance.IsHomebookInstanceCreated();

        // Assert
        result.ShouldBe(true);
    }

    [Test]
    public void IsSetupInstanceCreated_WithNonExistingSetupFile_ReturnsFalse()
    {
        // Arrange
        _fileSystemService.FileExists(Arg.Any<string>()).Returns(false);

        // Act
        var result = _instance.IsHomebookInstanceCreated();

        // Assert
        result.ShouldBe(false);
    }

    [Test]
    public async Task CreateSetupInstanceAsync_WithValidVersion_WritesVersionToFile()
    {
        // Arrange
        const string appVersion = "1.2.3";
        const string dataPath = "/data";
        const string expectedFilePath = "/data/.homebook";

        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(appVersion);
        _configuration.GetSection("Version").Returns(configSection);
        _applicationPathProvider.DataDirectory.Returns(dataPath);

        // Act
        await _instance.CreateHomebookInstanceAsync(_cancellationToken);

        // Assert
        await _fileSystemService.Received(1).FileWriteAllTextAsync(expectedFilePath, appVersion, _cancellationToken);
    }

    [Test]
    public async Task CreateSetupInstanceAsync_WithNullVersion_WritesEmptyStringToFile()
    {
        // Arrange
        const string dataPath = "/data";
        const string expectedFilePath = "/data/.homebook";

        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns((string)null!);
        _configuration.GetSection("Version").Returns(configSection);
        _applicationPathProvider.DataDirectory.Returns(dataPath);

        // Act
        await _instance.CreateHomebookInstanceAsync(_cancellationToken);

        // Assert
        await _fileSystemService.Received(1).FileWriteAllTextAsync(expectedFilePath, string.Empty, _cancellationToken);
    }

    [Test]
    public async Task CreateSetupInstanceAsync_WithWhitespaceVersion_WritesEmptyStringToFile()
    {
        // Arrange
        const string appVersion = "   ";
        const string dataPath = "/data";
        const string expectedFilePath = "/data/.homebook";

        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(appVersion);
        _configuration.GetSection("Version").Returns(configSection);
        _applicationPathProvider.DataDirectory.Returns(dataPath);

        // Act
        await _instance.CreateHomebookInstanceAsync(_cancellationToken);

        // Assert
        await _fileSystemService.Received(1).FileWriteAllTextAsync(expectedFilePath, string.Empty, _cancellationToken);
    }

    [Test]
    public async Task CreateSetupInstanceAsync_WithVersionWithWhitespace_WritesTrimmedVersionToFile()
    {
        // Arrange
        const string appVersionWithWhitespace = "  1.2.3  ";
        const string expectedTrimmedVersion = "1.2.3";
        const string dataPath = "/data";
        const string expectedFilePath = "/data/.homebook";

        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns(appVersionWithWhitespace);
        _configuration.GetSection("Version").Returns(configSection);
        _applicationPathProvider.DataDirectory.Returns(dataPath);

        // Act
        await _instance.CreateHomebookInstanceAsync(_cancellationToken);

        // Assert
        await _fileSystemService.Received(1)
            .FileWriteAllTextAsync(expectedFilePath,
                expectedTrimmedVersion,
                _cancellationToken);
    }

    [Test]
    public async Task CreateSetupInstanceAsync_WithNullConfigSection_WritesEmptyStringToFile()
    {
        // Arrange
        const string dataPath = "/data";
        const string expectedFilePath = "/data/.homebook";

        _configuration.GetSection("Version").Returns((IConfigurationSection)null!);
        _applicationPathProvider.DataDirectory.Returns(dataPath);

        // Act
        await _instance.CreateHomebookInstanceAsync(_cancellationToken);

        // Assert
        await _fileSystemService.Received(1)
            .FileWriteAllTextAsync(expectedFilePath,
                string.Empty,
                _cancellationToken);
    }

    [Test]
    public async Task CopySetupFilesAsync_WithSimpleWallpaperPathFiles_Return()
    {
        // Arrange
        _applicationPathProvider.ExecutableDirectory.Returns("/test/opt/homebook");
        string setupDirectory = Path.Combine(_applicationPathProvider.ExecutableDirectory, "setup");
        _fileSystemService.GetFilesInDirectoryAsync(setupDirectory, Arg.Any<CancellationToken>())
            .Returns([
                new FileInformation($"{setupDirectory}/wallpaper/bg001.jpg", 1024),
            ]);
        var testFileSystemService = new TestFileService();
        _fileSystemService.GetFolderPath(Arg.Any<SpecialFolder>())
            .Returns(testFileSystemService.GetFolderPath(SpecialFolder.MountedWallpaper));

        // Act
        await _instance.CopySetupFilesAsync(_cancellationToken);

        // Assert
        _fileSystemService.Received(1)
            .CopyFile("/test/opt/homebook/setup/wallpaper/bg001.jpg",
                "/test/lib/homebook/wallpaper/bg001.jpg",
                true);
    }

    [Test]
    public async Task CopySetupFilesAsync_WithWallpaperThemePath_Return()
    {
        // Arrange
        _applicationPathProvider.ExecutableDirectory.Returns("/test/opt/homebook");
        string setupDirectory = Path.Combine(_applicationPathProvider.ExecutableDirectory, "setup");
        _fileSystemService.GetFilesInDirectoryAsync(setupDirectory, Arg.Any<CancellationToken>())
            .Returns([
                new FileInformation($"{setupDirectory}/wallpaper/theme01/bg001.jpg", 1024),
            ]);
        var testFileSystemService = new TestFileService();
        _fileSystemService.GetFolderPath(Arg.Any<SpecialFolder>())
            .Returns(testFileSystemService.GetFolderPath(SpecialFolder.MountedWallpaper));

        // Act
        await _instance.CopySetupFilesAsync(_cancellationToken);

        // Assert
        _fileSystemService.Received(1)
            .CopyFile("/test/opt/homebook/setup/wallpaper/theme01/bg001.jpg",
                "/test/lib/homebook/wallpaper/theme01/bg001.jpg",
                true);
    }
}
