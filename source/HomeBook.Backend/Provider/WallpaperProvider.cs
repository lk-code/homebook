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

    private static readonly List<string> AllowedWallpaperExtension = [".webp", ".jpg", ".jpeg", ".png", ".theme"];

    public string GetAbsolutFilePathForWallpaper(string wallpaper)
    {
        string wallpaperDirectory = "";
        if (wallpaper.StartsWith("[mnt]"))
        {
            wallpaperDirectory = fileSystemService.GetFolderPath(SpecialFolder.MountedWallpaper);
        }
        else if (wallpaper.StartsWith("[img]"))
        {
            wallpaperDirectory = fileSystemService.GetFolderPath(SpecialFolder.ImageWallpaper);
        }

        string absoluteWallpaperPath = Path.Combine(wallpaperDirectory, wallpaper
            .Replace("[mnt]", "")
            .Replace("[img]", ""));
        return absoluteWallpaperPath;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<SystemWallpaperDto>> GetSystemWallpapersAsync(
        CancellationToken cancellationToken = default)
    {
        string mountedWallpaperDirectory = fileSystemService.GetFolderPath(SpecialFolder.MountedWallpaper);
        string imageWallpaperDirectory = fileSystemService.GetFolderPath(SpecialFolder.ImageWallpaper);

        List<FileInformation> mountedWallpaperFiles = new();
        List<FileInformation> imageWallpaperFiles = new();

        try
        {
            mountedWallpaperFiles = (await fileSystemService
                    .GetAllInDirectoryAsync(mountedWallpaperDirectory,
                        cancellationToken))
                .Where(e => Path.GetDirectoryName(e.FilePath) == mountedWallpaperDirectory)
                .Where(e => AllowedWallpaperExtension.Contains(Path.GetExtension(e.FilePath).ToLowerInvariant()))
                .Select(x => new FileInformation(x.FilePath.Replace(mountedWallpaperDirectory, "").TrimStart('/'),
                    x.SizeBytes,
                    x.IsDirectory))
                .ToList();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while loading mounted wallpapers");
        }

        try
        {
            imageWallpaperFiles = (await fileSystemService
                    .GetAllInDirectoryAsync(imageWallpaperDirectory,
                        cancellationToken))
                .Where(e => Path.GetDirectoryName(e.FilePath) == imageWallpaperDirectory)
                .Where(e => AllowedWallpaperExtension.Contains(Path.GetExtension(e.FilePath).ToLowerInvariant()))
                .Select(x => new FileInformation(x.FilePath.Replace(imageWallpaperDirectory, "").TrimStart('/'),
                    x.SizeBytes,
                    x.IsDirectory))
                .ToList();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while loading image wallpapers");
        }

        List<string> allWallpaperFiles = mountedWallpaperFiles.Select(x => $"[mnt]{x.FilePath}")
            .Concat(imageWallpaperFiles.Select(x => $"[img]{x.FilePath}"))
            .ToList();
        List<SystemWallpaperDto> systemWallpapers = new();
        foreach (string wallpaperFile in allWallpaperFiles)
        {
            string wallpaperPath = "";

            if (wallpaperFile.StartsWith("[mnt]"))
            {
                wallpaperPath = Path.Combine(mountedWallpaperDirectory, wallpaperFile.Replace("[mnt]", ""));
            }
            else if (wallpaperFile.StartsWith("[img]"))
            {
                wallpaperPath = Path.Combine(imageWallpaperDirectory, wallpaperFile.Replace("[img]", ""));
            }

            if (fileSystemService.FileExists(wallpaperPath))
            {
                // is path a file => its the wallpaper file

                systemWallpapers.Add(new SystemWallpaperDto(wallpaperFile));
                continue;
            }
            else if (fileSystemService.DirectoryExists(wallpaperPath))
            {
                // is path a directory => its a wallpaper set which contains multiple wallpaper

                string wallpaperIndexFilePath = Path.Combine(wallpaperPath, "theme.json");

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
                                .Select(x => Path.Combine(wallpaperFile, x.GetValue<string>()))
                                .ToList()
                        );

                    systemWallpapers.Add(new SystemWallpaperDto(wallpaperFile,
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
