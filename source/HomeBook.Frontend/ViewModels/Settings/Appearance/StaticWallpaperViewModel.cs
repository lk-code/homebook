namespace HomeBook.Frontend.ViewModels.Settings.Appearance;

public class StaticWallpaperViewModel(Uri? absoluteUri)
{
    public Uri? AbsoluteUri { get; set; } = absoluteUri;
}
