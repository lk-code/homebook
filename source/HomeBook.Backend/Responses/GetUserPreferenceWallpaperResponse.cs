namespace HomeBook.Backend.Responses;

public record GetUserPreferenceWallpaperResponse(
    string Config,
    string Type,
    string Wallpaper);
