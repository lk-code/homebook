namespace HomeBook.Frontend.Abstractions.Models;

public record WallpaperDto(WallpaperType Type,
    Guid? MediaId,
    Uri? Url);
