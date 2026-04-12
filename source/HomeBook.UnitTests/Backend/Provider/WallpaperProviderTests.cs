using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Enums;
using HomeBook.Backend.Provider;
using HomeBook.UnitTests.TestCore.Backend.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HomeBook.UnitTests.Backend.Provider;

[TestFixture]
public class WallpaperProviderTests
{
    private ILogger<WallpaperProvider> _logger;
    private IStorageProvider _storageProvider = null!;
    private IFileSystemService _fileSystemService = null!;
    private IApplicationPathProvider _applicationPathProvider = null!;
    private WallpaperProvider _instance = null!;

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
        _logger = factory.CreateLogger<WallpaperProvider>();

        _storageProvider = Substitute.For<IStorageProvider>();
        _fileSystemService = new TestFileService();
        _applicationPathProvider = new TestFileService();

        _instance = new WallpaperProvider(_logger,
            _storageProvider,
            _fileSystemService,
            _applicationPathProvider);
    }

    [Test]
    public async Task GetSystemWallpapersAsync_Return()
    {
        // Arrange
        string wallpaperDirectory = _fileSystemService.GetFolderPath(SpecialFolder.MountedWallpaper);
        foreach (var registeredWallpaper in WallpaperProvider.WallpaperFiles)
        {
            if (!(Path.GetExtension(registeredWallpaper.Value).Equals(".theme", StringComparison.OrdinalIgnoreCase)))
            {
                string wallpaperFile = Path.Combine(wallpaperDirectory, registeredWallpaper.Value);
                (_fileSystemService as TestFileService).SetFileContentSilently(wallpaperFile, "wallpaper-content");
            }
            else
            {
                string wallpaperTheme = Path.Combine(wallpaperDirectory, registeredWallpaper.Value);
                (_fileSystemService as TestFileService).AddDirectorySilently(wallpaperTheme);
                string wallpaperIndex = Path.Combine(wallpaperTheme, "theme.json");
                string wallpaperImg01 = Path.Combine(wallpaperTheme, "Mountains.Light@1x.webp");
                (_fileSystemService as TestFileService).SetFileContentSilently(wallpaperImg01, "wallpaper-content");
                string wallpaperImg02 = Path.Combine(wallpaperTheme, "Mountains.Light@3x.webp");
                (_fileSystemService as TestFileService).SetFileContentSilently(wallpaperImg02, "wallpaper-content");
                string wallpaperImg03 = Path.Combine(wallpaperTheme, "Mountains.Dark@1x.webp");
                (_fileSystemService as TestFileService).SetFileContentSilently(wallpaperImg03, "wallpaper-content");
                string wallpaperImg04 = Path.Combine(wallpaperTheme, "Mountains.Dark@3x.webp");
                (_fileSystemService as TestFileService).SetFileContentSilently(wallpaperImg04, "wallpaper-content");
                (_fileSystemService as TestFileService).SetFileContentSilently(wallpaperIndex,
                    """
                    {
                        "light": [
                            "MountainsTest.Light@1x.webp",
                            "MountainsTest.Light@3x.webp"
                        ],
                        "dark": [
                            "MountainsTest.Dark@1x.webp",
                            "MountainsTest.Dark@3x.webp"
                        ],
                        "neutral": [
                            "MountainsTest.Neutral@1x.webp",
                            "MountainsTest.Neutral@2x.webp",
                            "MountainsTest.Neutral@3x.webp"
                        ],
                        "12am": [ "MountainsTest.00@1x.webp" ],
                        "1am": [ "MountainsTest.01@1x.webp" ],
                        "2am": [ "MountainsTest.02@1x.webp" ],
                        "3am": [ "MountainsTest.03@1x.webp" ],
                        "4am": [ "MountainsTest.04@1x.webp" ],
                        "5am": [ "MountainsTest.05@1x.webp" ],
                        "6am": [ "MountainsTest.06@1x.webp" ],
                        "7am": [ "MountainsTest.07@1x.webp" ],
                        "8am": [ "MountainsTest.08@1x.webp" ],
                        "9am": [ "MountainsTest.09@1x.webp" ],
                        "10am": [ "MountainsTest.10@1x.webp" ],
                        "11am": [ "MountainsTest.11@1x.webp" ],
                        "12pm": [ "MountainsTest.12@1x.webp" ],
                        "1pm": [ "MountainsTest.13@1x.webp" ],
                        "2pm": [ "MountainsTest.14@1x.webp" ],
                        "3pm": [ "MountainsTest.15@1x.webp" ],
                        "4pm": [ "MountainsTest.16@1x.webp" ],
                        "5pm": [ "MountainsTest.17@1x.webp" ],
                        "6pm": [ "MountainsTest.18@1x.webp" ],
                        "7pm": [ "MountainsTest.19@1x.webp" ],
                        "8pm": [ "MountainsTest.20@1x.webp" ],
                        "9pm": [ "MountainsTest.21@1x.webp" ],
                        "10pm": [ "MountainsTest.22@1x.webp" ],
                        "11pm": [ "MountainsTest.23@1x.webp" ],
                        "2300": [ "Invalid.webp" ],
                        "1200": [ "Invalid.webp" ],
                        "system": [ "Invalid.webp" ]
                    }
                    """);
            }
        }

        // Act & Assert
        var wallpapers = await _instance.GetSystemWallpapersAsync(CancellationToken.None);
        wallpapers.ShouldNotBeNull();

        wallpapers.Count.ShouldBe(WallpaperProvider.WallpaperFiles.Count);
        foreach (var dto in wallpapers)
        {
            dto.Path.ShouldNotStartWith(wallpaperDirectory);
            if (dto.Configuration is not null)
            {
                foreach (var configByKey in dto.Configuration)
                {
                    foreach (var configFilePath in configByKey.Value)
                    {
                        configFilePath.ShouldNotStartWith(wallpaperDirectory);
                    }
                }
            }
        }
    }
}
