namespace HomeBook.Backend.Abstractions.Models.UserPreferences;

public record WallpaperConfiguration(
    string Key,
    Dictionary<string, List<string>>? Configuration,
    string Type,
    string WallpaperKey);
