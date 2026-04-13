using HomeBook.Frontend.Abstractions.Models;

namespace HomeBook.UnitTests.Frontend.Abstractions.Models;

[TestFixture]
public class WallpaperConfigurationTests
{
    [TestCase("{usrwp}-{efc3e0ee-0ae5-441d-8d96-f86fa9e2df8e}",
        WallpaperType.Uploaded,
        "efc3e0ee-0ae5-441d-8d96-f86fa9e2df8e",
        null,
        null)]
    [TestCase("{dynwp}-{TestWallpaper}",
        WallpaperType.Dynamic,
        null,
        null,
        "TestWallpaper")]
    [TestCase("{stawp}-{/wallpaper.theme}",
        WallpaperType.Static,
        null,
        "/wallpaper.theme",
        null)]
    public void ParseAndToString_WithDifferentValues_Return(string config,
        WallpaperType expectedType,
        string? expectedMediaIdVal,
        string? expectedStaticWallpaperUrl,
        string? expectedDynamicWallpaperName)
    {
        Guid? expectedMediaId = Guid.TryParse(expectedMediaIdVal, out Guid parsed) ? parsed : null;

        // test Parse()
        WallpaperConfiguration wallpaperConfiguration = WallpaperConfiguration.Parse(config);
        wallpaperConfiguration.ShouldNotBeNull();
        wallpaperConfiguration.Type.ShouldBe(expectedType);
        wallpaperConfiguration.MediaId.ShouldBe(expectedMediaId);
        wallpaperConfiguration.StaticWallpaperUrl.ShouldBe(expectedStaticWallpaperUrl);
        wallpaperConfiguration.DynamicWallpaperName.ShouldBe(expectedDynamicWallpaperName);

        // test ToString()
        string result = wallpaperConfiguration.ToString();
        result.ShouldNotBeNull();
        result.ShouldBe(config);
    }
}
