using HomeBook.Frontend.Abstractions.Models;
using HomeBook.Frontend.Module.Kitchen.ViewModels;
using HomeBook.Frontend.ViewModels.Settings.Appearance;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace HomeBook.Frontend.Pages.Settings.Appearance;

public partial class Overview : ComponentBase
{
    private const string wallpaperModuleKey = "homebook.core.wallpaper";
    private bool _isUploadingImage = false;
    private InputFile _fileInput;

    private List<string> _dynamicWallpaperThumbImageUrls =
    [
        "https://data.mactechnews.de/367704.jpg",
        "https://static.wikia.nocookie.net/windowswallpaper/images/0/0a/Windows_7_-_img0.jpg/revision/latest?cb=20250210043334",
        "https://cdn.wallpapersafari.com/71/97/x7lcOr.jpg",
        "https://data.mactechnews.de/367704.jpg",
        "https://static.wikia.nocookie.net/windowswallpaper/images/0/0a/Windows_7_-_img0.jpg/revision/latest?cb=20250210043334",
        "https://cdn.wallpapersafari.com/71/97/x7lcOr.jpg",
        "https://data.mactechnews.de/367704.jpg",
        "https://static.wikia.nocookie.net/windowswallpaper/images/0/0a/Windows_7_-_img0.jpg/revision/latest?cb=20250210043334",
        "https://cdn.wallpapersafari.com/71/97/x7lcOr.jpg"
    ];

    private readonly List<StaticWallpaperViewModel> _staticWallpaperThumbImageUrls = [];
    private readonly List<MediaItemViewModel> _uploadedWallpaperThumbImageUrls = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        CancellationToken cancellationToken = CancellationToken.None;
        await LoadWallpapersAsync(cancellationToken);
    }

    private async Task LoadWallpapersAsync(CancellationToken cancellationToken)
    {
        List<WallpaperDto> wallpapers = await WallpaperService.GetAllWallpapersAsync(cancellationToken);
        string wallpaperEndpoint = "/system/wallpaper";

        _uploadedWallpaperThumbImageUrls.Clear();
        _staticWallpaperThumbImageUrls.Clear();

        foreach (WallpaperDto wp in wallpapers)
        {
            try
            {
                switch (wp.Type)
                {
                    case WallpaperType.Static:
                    {
                        string escapedWallpaperName = Uri.EscapeDataString(wp.StaticWallpaper)
                            .Replace(".", "%2E");
                        Uri absoluteWallpaperUri = AppUriProvider
                            .GetAbsoluteUri($"{wallpaperEndpoint}/{escapedWallpaperName}");
                        _staticWallpaperThumbImageUrls.Add(new StaticWallpaperViewModel(wp.StaticWallpaperName,
                            absoluteWallpaperUri));
                    }
                        break;
                    case WallpaperType.Uploaded:
                    {
                        Guid mediaId = wp.MediaId!.Value;
                        Uri? absoluteUri = await MediaService.GetUrlForMediaItemAsync(mediaId,
                            cancellationToken);
                        _uploadedWallpaperThumbImageUrls.Add(new MediaItemViewModel(mediaId,
                            absoluteUri,
                            0));
                    }
                        break;
                    case WallpaperType.Dynamic:
                    {
                    }
                        break;
                }
            }
            catch (Exception err)
            {
            }
        }

        StateHasChanged();
    }

    private async Task UploadWallpaperImagesAsync(InputFileChangeEventArgs args)
    {
        try
        {
            _isUploadingImage = true;
            StateHasChanged();

            CancellationToken cancellationToken = CancellationToken.None;
            Guid? userWallpaperStorageScopeId = await FileStorageRegistration.GetScopeIdForModuleAsync(
                wallpaperModuleKey,
                "UserWallpaper",
                cancellationToken);
            if (userWallpaperStorageScopeId is null)
                return;

            IBrowserFile file = args.File;
            using Stream stream = file.OpenReadStream((50 * 1024 * 1024));
            using MemoryStream ms = new();

            await stream.CopyToAsync(ms);
            byte[] fileContent = ms.ToArray();

            Guid mediaItemId = await FileStorageService.WriteFileAllBytesAsync(userWallpaperStorageScopeId.Value,
                file.Name,
                fileContent,
                cancellationToken);

            await AddUserWallpaperToGalleryAsync(mediaItemId, cancellationToken);

            // TODO: set mediaId as current wallpaper
            await SetAsWallpaperAsync(mediaItemId);

            StateHasChanged();
        }
        catch (Exception)
        {
        }
        finally
        {
            _isUploadingImage = false;
            StateHasChanged();
        }
    }

    private async Task AddUserWallpaperToGalleryAsync(Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        Uri? absoluteUri = await MediaService.GetUrlForMediaItemAsync(mediaItemId,
            cancellationToken);
        _uploadedWallpaperThumbImageUrls.Add(new MediaItemViewModel(mediaItemId,
            absoluteUri,
            0));

        StateHasChanged();
    }

    private async Task SetAsWallpaperAsync(StaticWallpaperViewModel wp)
    {
        // set static wallpaper

        CancellationToken cancellationToken = CancellationToken.None;

        await UserPreferencesProvider.SetStaticWallpaperAsync(wp.Wallpaper,
            cancellationToken);
    }

    private async Task SetAsWallpaperAsync(Guid mediaId)
    {
        // set own uploaded wallpaper

        CancellationToken cancellationToken = CancellationToken.None;

        await UserPreferencesProvider.SetUploadedWallpaperAsync(mediaId,
            cancellationToken);
    }

    private async Task SetAsWallpaperAsync(string wp)
    {
        // set dynamic wallpaper

        CancellationToken cancellationToken = CancellationToken.None;

        await UserPreferencesProvider.SetDynamicWallpaperAsync(wp,
            cancellationToken);
    }
}
