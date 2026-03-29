using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace HomeBook.Frontend.Pages.Settings.Appearance;

public partial class Overview : ComponentBase
{
    private const string wallpaperModuleKey = "homebook.core.wallpaper";
    private bool _isUploadingImage = false;
    private InputFile _fileInput;

    private string[] _dynamicWallpaperThumbImageUrls =
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

    private string[] _staticWallpaperThumbImageUrls =
    [
        "/img/bg/Mountains.Dark@1x.webp",
        "/img/bg/Mountains.Light@1x.webp"
    ];

    private List<string> _lastOwnWallpaperThumbImageUrls =
    [
        "https://cdn.wallpapersafari.com/71/97/x7lcOr.jpg"
    ];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        // TODO: load all wallpapers
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
        // TODO: 1. add mediaId to user wallpapers via backend client

        // TODO: 2. reload user wallpapers from backend
    }
}
