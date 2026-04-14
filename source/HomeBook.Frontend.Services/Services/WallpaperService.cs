using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.Services.Mappings;

namespace HomeBook.Frontend.Services.Services;

public class WallpaperService(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : IWallpaperService
{
    public async Task<List<WallpaperDto>> GetAllWallpapersAsync(CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        GetSystemWallpapersResponse? response = await backendClient.System.Wallpaper.GetAsync(x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        List<WallpaperDto> wallpapers = [];

        // static wallpapers
        foreach (StaticWallpaperEntry staticWallpaper in response.SystemWallpapers)
        {
            if (staticWallpaper.Configuration is null
                && !string.IsNullOrEmpty(staticWallpaper.FilePath))
            {
                wallpapers.Add(new WallpaperDto(WallpaperType.Static,
                    null,
                    staticWallpaper.FilePath,
                    staticWallpaper.FilePath));
                continue;
            }

            var configuration = staticWallpaper.Configuration;
            if (configuration is null
                || !configuration.AdditionalData.Any())
                continue;

            Dictionary<string, List<string>>? wpConfig = configuration.AdditionalData.MapToDictionary();
            string firstWallpaperImage = wpConfig.First().Value.First();
            wallpapers.Add(new WallpaperDto(WallpaperType.Static,
                null,
                firstWallpaperImage,
                staticWallpaper.FilePath));
        }

        // uploaded wallpapers
        wallpapers.AddRange((response.UploadedWallpapers ?? []).Select(wp =>
            new WallpaperDto(WallpaperType.Uploaded,
                wp.MediaId,
                null,
                null)));

        return wallpapers;
    }
}
