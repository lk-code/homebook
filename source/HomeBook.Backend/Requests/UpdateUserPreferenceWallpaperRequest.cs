using System.ComponentModel.DataAnnotations;

namespace HomeBook.Backend.Requests;

public record UpdateUserPreferenceWallpaperRequest([Required] string WallpaperConfiguration);

