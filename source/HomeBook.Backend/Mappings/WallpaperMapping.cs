using HomeBook.Backend.Abstractions.Models;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Mappings;

public static class WallpaperMapping
{
    public static StaticWallpaperEntry ToResponse(this SystemWallpaperDto dto) =>
        new(dto.Path,
            dto.Configuration);
}
