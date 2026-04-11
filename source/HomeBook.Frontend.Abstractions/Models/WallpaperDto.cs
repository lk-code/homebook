namespace HomeBook.Frontend.Abstractions.Models;

public record WallpaperDto(string? Key,
    WallpaperType Type,
    Guid? MediaId,
    Uri? Url);
