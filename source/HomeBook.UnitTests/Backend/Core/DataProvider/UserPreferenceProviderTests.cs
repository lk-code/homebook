using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.UserPreferences;
using HomeBook.Backend.Core.DataProvider;
using HomeBook.Backend.Core.DataProvider.Exceptions;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Data.Validators;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HomeBook.UnitTests.Backend.Core.DataProvider;

[TestFixture]
public class UserPreferenceProviderTests
{
    private CancellationToken _cancellationToken;
    private ILogger<UserPreferenceProvider> _logger;
    private IUserPreferenceRepository _userPreferenceRepository;
    private IWallpaperProvider _wallpaperProvider;
    private UserPreferenceProvider _instance;

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
        _logger = factory.CreateLogger<UserPreferenceProvider>();
        _userPreferenceRepository = Substitute.For<IUserPreferenceRepository>();
        _wallpaperProvider = Substitute.For<IWallpaperProvider>();
        _instance = new UserPreferenceProvider(
            _logger,
            _userPreferenceRepository,
            new UserPreferenceValidator(),
            _wallpaperProvider);
    }

    #region GetUserPreferredLocaleAsync

    [Test]
    public async Task GetUserPreferredLocaleAsync_WhenPreferenceExists_ReturnsValue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId, "LOCALE", _cancellationToken)
            .Returns(new UserPreference
            {
                UserId = userId,
                Key = "LOCALE",
                Value = "de-AT"
            });

        // Act
        string? result = await _instance.GetUserPreferredLocaleAsync(userId, _cancellationToken);

        // Assert
        Assert.That(result, Is.EqualTo("de-AT"));
        await _userPreferenceRepository.Received(1)
            .GetPreferenceForUserByKeyAsync(userId, "LOCALE", _cancellationToken);
    }

    [Test]
    public async Task GetUserPreferredLocaleAsync_WhenPreferenceNotFound_ReturnsNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId, "LOCALE", _cancellationToken)
            .Returns((UserPreference?)null);

        // Act
        string? result = await _instance.GetUserPreferredLocaleAsync(userId, _cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
        await _userPreferenceRepository.Received(1)
            .GetPreferenceForUserByKeyAsync(userId, "LOCALE", _cancellationToken);
    }

    #endregion

    #region SetUserPreferredLocaleAsync

    [Test]
    public async Task SetUserPreferredLocaleAsync_WithValidLocale_CallsSetPreferenceWithCorrectValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string locale = "en-US";

        // Act
        await _instance.SetUserPreferredLocaleAsync(userId, locale, _cancellationToken);

        // Assert
        await _userPreferenceRepository.Received(1)
            .SetPreferenceAsync(
                Arg.Is<UserPreference>(p =>
                    p.UserId == userId &&
                    p.Key == "LOCALE" &&
                    p.Value == locale),
                _cancellationToken);
    }

    #endregion

    #region GetUserWallpaperAsync

    [Test]
    public async Task GetUserWallpaperAsync_WhenPreferenceExists_ReturnsCorrectConfiguration()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId, "WALLPAPER", _cancellationToken)
            .Returns(new UserPreference
            {
                UserId = userId,
                Key = "WALLPAPER",
                Value = "{dynwp}-{Sunrise}"
            });

        // Act
        WallpaperConfiguration? result = await _instance.GetUserWallpaperAsync(userId, _cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("dynwp"));
        Assert.That(result.WallpaperKey, Is.EqualTo("Sunrise"));
        await _userPreferenceRepository.Received(1)
            .GetPreferenceForUserByKeyAsync(userId, "WALLPAPER", _cancellationToken);
    }

    [Test]
    public async Task GetUserWallpaperAsync_WhenPreferenceNotFound_ReturnsNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId, "WALLPAPER", _cancellationToken)
            .Returns((UserPreference?)null);

        // Act
        WallpaperConfiguration? result = await _instance.GetUserWallpaperAsync(userId, _cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
        await _userPreferenceRepository.Received(1)
            .GetPreferenceForUserByKeyAsync(userId, "WALLPAPER", _cancellationToken);
    }

    [Test]
    public async Task GetUserWallpaperAsync_WithKeyContainingDash_SplitsOnFirstDashOnly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId, "WALLPAPER", _cancellationToken)
            .Returns(new UserPreference
            {
                UserId = userId,
                Key = "WALLPAPER",
                Value = "{stawp}-{Community/Spring.theme}"
            });

        // Act
        WallpaperConfiguration? result = await _instance.GetUserWallpaperAsync(userId, _cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("stawp"));
        Assert.That(result.WallpaperKey, Is.EqualTo("Community/Spring.theme"));
    }

    #endregion

    #region SetUserWallpaperAsync

    [Test]
    public async Task SetUserWallpaperAsync_WithValidDynwpConfiguration_CallsSetPreferenceWithCorrectValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string wallpaperConfig = "{dynwp}-{Sunrise}";

        // Act
        await _instance.SetUserWallpaperAsync(userId, wallpaperConfig, _cancellationToken);

        // Assert
        await _userPreferenceRepository.Received(1)
            .SetPreferenceAsync(
                Arg.Is<UserPreference>(p =>
                    p.UserId == userId &&
                    p.Key == "WALLPAPER" &&
                    p.Value == "{dynwp}-{Sunrise}"),
                _cancellationToken);
    }

    [Test]
    public async Task SetUserWallpaperAsync_WithValidStawpConfiguration_CallsSetPreferenceWithCorrectValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string wallpaperConfig = "{stawp}-{Mountains.theme}";

        // Act
        await _instance.SetUserWallpaperAsync(userId, wallpaperConfig, _cancellationToken);

        // Assert
        await _userPreferenceRepository.Received(1)
            .SetPreferenceAsync(
                Arg.Is<UserPreference>(p =>
                    p.UserId == userId &&
                    p.Key == "WALLPAPER" &&
                    p.Value == "{stawp}-{Mountains.theme}"),
                _cancellationToken);
    }

    [Test]
    public async Task SetUserWallpaperAsync_WithValidUsrwpConfiguration_CallsSetPreferenceWithCorrectValues()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid mediaId = Guid.NewGuid();
        string wallpaperConfig = "{usrwp}-{" + mediaId.ToString() + "}";

        // Act
        await _instance.SetUserWallpaperAsync(userId, wallpaperConfig, _cancellationToken);

        // Assert
        await _userPreferenceRepository.Received(1)
            .SetPreferenceAsync(
                Arg.Is<UserPreference>(p =>
                    p.UserId == userId &&
                    p.Key == "WALLPAPER" &&
                    p.Value == "{usrwp}-{" + mediaId.ToString() + "}"),
                _cancellationToken);
    }

    [Test]
    public void SetUserWallpaperAsync_WithInvalidFormat_ThrowsInvalidPreferenceException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string wallpaperConfig = "dynwp-Sunrise";

        // Act & Assert
        Assert.ThrowsAsync<InvalidPreferenceException>(() =>
            _instance.SetUserWallpaperAsync(userId, wallpaperConfig, _cancellationToken));
    }

    [Test]
    public void SetUserWallpaperAsync_WithUnknownType_ThrowsInvalidPreferenceException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string wallpaperConfig = "{xxxxx}-{somekey}";

        // Act & Assert
        Assert.ThrowsAsync<InvalidPreferenceException>(() =>
            _instance.SetUserWallpaperAsync(userId, wallpaperConfig, _cancellationToken));
    }

    [Test]
    public void SetUserWallpaperAsync_WithInvalidFormat_DoesNotCallRepository()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        Assert.ThrowsAsync<InvalidPreferenceException>(() =>
            _instance.SetUserWallpaperAsync(userId, "dynwp-Sunrise", _cancellationToken));

        // Assert
        _userPreferenceRepository.DidNotReceive()
            .SetPreferenceAsync(Arg.Any<UserPreference>(), Arg.Any<CancellationToken>());
    }

    #endregion
}
