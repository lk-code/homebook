namespace HomeBook.Backend.Responses;

public record GetSystemWallpapersResponse(
    List<StaticWallpaperEntry> SystemWallpapers,
    List<MediaWallpaperEntry> UploadedWallpapers);

public record StaticWallpaperEntry(
    string Name,
    string FilePath,
    Dictionary<string, List<string>>? Configuration);

public record MediaWallpaperEntry(Guid MediaId);
