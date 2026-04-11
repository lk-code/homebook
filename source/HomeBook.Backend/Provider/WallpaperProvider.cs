using System.Text.Json.Nodes;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Enums;
using HomeBook.Backend.Abstractions.Models;

namespace HomeBook.Backend.Provider;

/// <inheritdoc/>
public class WallpaperProvider(
    ILogger<WallpaperProvider> logger,
    IStorageProvider storageProvider,
    IFileSystemService fileSystemService,
    IApplicationPathProvider applicationPathProvider) : IWallpaperProvider
{
    private static readonly HashSet<string> ValidWallpaperConfigKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "light",
        "dark",
        "neutral",
        "12am",
        "1am",
        "2am",
        "3am",
        "4am",
        "5am",
        "6am",
        "7am",
        "8am",
        "9am",
        "10am",
        "11am",
        "12pm",
        "1pm",
        "2pm",
        "3pm",
        "4pm",
        "5pm",
        "6pm",
        "7pm",
        "8pm",
        "9pm",
        "10pm",
        "11pm"
    };

    public static readonly Dictionary<string, string> WallpaperFiles = new()
    {
        {
            "Mountains", "Mountains.theme"
        },
        {
            "Flickering", "flickering.jpg"
        }
    };

    public string GetAbsolutFilePathForWallpaper(string wallpaper)
    {
        string wallpaperDirectory = fileSystemService.GetFolderPath(SpecialFolder.Wallpaper);
        string absoluteWallpaperPath = Path.Combine(wallpaperDirectory, wallpaper);
        return absoluteWallpaperPath;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<SystemWallpaperDto>> GetSystemWallpapersAsync(
        CancellationToken cancellationToken = default)
    {
        string wallpaperDirectory = fileSystemService.GetFolderPath(SpecialFolder.Wallpaper);
        List<SystemWallpaperDto> systemWallpapers = new();
        foreach (var wallpaperFile in WallpaperFiles)
        {
            string absoluteWallpaperPath = Path.Combine(wallpaperDirectory, wallpaperFile.Value);

            if (fileSystemService.FileExists(absoluteWallpaperPath))
            {
                // is path a file => its the wallpaper file

                systemWallpapers.Add(new SystemWallpaperDto(wallpaperFile.Key,
                    wallpaperFile.Value));
                continue;
            }
            else if (fileSystemService.DirectoryExists(absoluteWallpaperPath))
            {
                // is path a directory => its a wallpaper set which contains multiple wallpaper

                string wallpaperIndexFilePath = Path.Combine(absoluteWallpaperPath, "theme.json");

                try
                {
                    string content = await fileSystemService
                        .FileReadAllTextAsync(wallpaperIndexFilePath,
                            cancellationToken);
                    JsonObject? jsonObj = JsonNode.Parse(content)
                        ?.AsObject();

                    if (jsonObj is null)
                        continue;

                    Dictionary<string, List<string>> result = jsonObj
                        .Where(prop => ValidWallpaperConfigKeys.Contains(prop.Key))
                        .ToDictionary(
                            prop => prop.Key,
                            prop => prop.Value!.AsArray()
                                .Select(x => Path.Combine(wallpaperFile.Value, x.GetValue<string>()))
                                .ToList()
                        );

                    systemWallpapers.Add(new SystemWallpaperDto(wallpaperFile.Key,
                        wallpaperFile.Value,
                        result));
                }
                catch (Exception err)
                {
                    logger.LogError(err, "Error reading wallpaper index file at {0}", wallpaperIndexFilePath);
                    continue;
                }
            }
        }

        return systemWallpapers;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<MediaItemWallpaperDto>> GetUploadedWallpapersAsync(
        CancellationToken cancellationToken = default)
    {
        string fullScope = "homebook.core.wallpaper.userwallpaper";
        Guid? wallpaperStorageScopeId = await storageProvider.GetScopeIdByFullNameAsync(fullScope,
            cancellationToken);

        if (wallpaperStorageScopeId is null)
            throw new ArgumentNullException(nameof(wallpaperStorageScopeId),
                "Wallpaper storage scope is not registered");

        Guid[] mediaItemIds = await storageProvider.GetAllInScopeAsync(wallpaperStorageScopeId.Value,
            cancellationToken);

        return mediaItemIds.Select(id => new MediaItemWallpaperDto(id))
            .ToArray();
    }
}
