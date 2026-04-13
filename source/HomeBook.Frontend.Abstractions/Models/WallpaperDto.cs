namespace HomeBook.Frontend.Abstractions.Models;

public record WallpaperDto(
    WallpaperType Type,
    Guid? MediaId,
    string? StaticWallpaper,
    string? StaticWallpaperName);
