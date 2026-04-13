using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Models;
using Microsoft.Kiota.Abstractions.Serialization;

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

            UntypedArray? configWallpaper = configuration.AdditionalData.First().Value as UntypedArray;
            if (configWallpaper is null
                || !configWallpaper.GetValue().Any()
               )
                continue;

            UntypedNode wallpaperNode = configWallpaper.GetValue().First();
            string wallpaperFileVaue = (wallpaperNode as UntypedString).GetValue();
            wallpapers.Add(new WallpaperDto(WallpaperType.Static,
                null,
                wallpaperFileVaue,
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
