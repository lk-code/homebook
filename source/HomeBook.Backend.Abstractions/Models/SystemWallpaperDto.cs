namespace HomeBook.Backend.Abstractions.Models;

/// <summary>
///
/// </summary>
/// <param name="Key"></param>
/// <param name="Path"></param>
/// <param name="Configuration"></param>
public record SystemWallpaperDto(string Key,
    string Path,
    Dictionary<string, List<string>>? Configuration = null);
