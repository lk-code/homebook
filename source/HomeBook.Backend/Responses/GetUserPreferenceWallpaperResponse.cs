namespace HomeBook.Backend.Responses;

public record GetUserPreferenceWallpaperResponse(
    string Key,
    Dictionary<string, List<string>>? Configuration,
    string Type,
    string Wallpaper);
