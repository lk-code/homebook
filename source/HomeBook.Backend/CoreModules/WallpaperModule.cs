using HomeBook.Backend.Modules.Abstractions;

namespace HomeBook.Backend.CoreModules;

public class WallpaperModule : IModule,
    IBackendModuleStorageRegistrar
{
    public string Name { get; } = "HomeBook";
    public string Description { get; } = "Provides core functionality for HomeBook wallpaper management";
    public string Key { get; } = "homebook.core.wallpaper";
    public string Author { get; } = "HomeBook";
    public Version Version { get; } = new Version(1, 0, 0);

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public void RegisterStorage(IStorageBuilder storageBuilder,
        IConfiguration configuration)
    {
        storageBuilder.RegisterStorage("UserWallpaper");
    }
}
