namespace HomeBook.Frontend.ViewModels.Settings.Appearance;

public class StaticWallpaperViewModel(string key, Uri? absoluteUri)
{
    public string Key { get; set; } = key;

    public Uri? AbsoluteUri { get; set; } = absoluteUri;
}
