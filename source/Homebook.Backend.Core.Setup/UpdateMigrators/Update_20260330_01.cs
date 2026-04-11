using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.UserManagement;
using HomeBook.Backend.Abstractions.Models.UserPreferences;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup.UpdateMigrators;

public class Update_20260330_01(
    ILogger<Update_20250910_01> logger,
    IUserProvider userProvider,
    IUserPreferenceProvider userPreferenceProvider) : IUpdateMigrator
{
    public const string DEFAULT_WALLPAPER = "{stawp}-{Mountains.theme}";

    /// <inheritdoc />
    public Version Version { get; } = new(1, 0, 169);

    /// <inheritdoc />
    public string Description { get; } = "Add Wallpaper Configuration";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        IEnumerable<UserInfo> users = await userProvider.GetAllAsync(cancellationToken);
        foreach (UserInfo userInfo in users)
        {
            WallpaperConfiguration? wallpaperConfiguration = await userPreferenceProvider
                .GetUserWallpaperAsync(userInfo.Id,
                    cancellationToken);
            if (wallpaperConfiguration is not null)
                continue;

            await userPreferenceProvider.SetUserWallpaperAsync(userInfo.Id,
                DEFAULT_WALLPAPER,
                cancellationToken);
        }
    }
}
