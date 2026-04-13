namespace HomeBook.Frontend.ViewModels.Settings.Appearance;

public class StaticWallpaperViewModel(string? wallpaper, Uri? absoluteUri)
{
    public string? Wallpaper { get; } = wallpaper;
    public Uri? AbsoluteUri { get; } = absoluteUri;
}
