using System.Text;

namespace HomeBook.Frontend.Abstractions.Models;

public class WallpaperConfiguration(
    WallpaperType type,
    Guid? mediaId,
    string? staticWallpaperUrl,
    string? dynamicWallpaperName)
{
    public WallpaperType Type { get; } = type;
    public Guid? MediaId { get; } = mediaId;
    public string? StaticWallpaperUrl { get; } = staticWallpaperUrl;
    public string? DynamicWallpaperName { get; } = dynamicWallpaperName;

    public static WallpaperConfiguration Parse(string config)
    {
        WallpaperType? type = null;
        Guid? mediaId = null;
        string? staticWallpaperUrl = null;
        string? dynamicWallpaperName = null;

        string[] parts = config.Split("}-{", 2);
        string configType = parts[0].TrimStart('{');
        string configValue = parts[1].TrimEnd('}');

        switch (configType.ToLowerInvariant())
        {
            case "stawp":
            {
                type = WallpaperType.Static;
                staticWallpaperUrl = configValue;
            }
                break;
            case "dynwp":
            {
                type = WallpaperType.Dynamic;
                dynamicWallpaperName = configValue;
            }
                break;
            case "usrwp":
            {
                type = WallpaperType.Uploaded;
                mediaId = Guid.Parse(configValue);
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(configType), configType, null);
        }

        return new WallpaperConfiguration(
            type ?? throw new ArgumentException("wallpaper type is required"),
            mediaId,
            staticWallpaperUrl,
            dynamicWallpaperName);
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("{");
        switch (Type)
        {
            case WallpaperType.Static:
                sb.Append("stawp");
                break;
            case WallpaperType.Dynamic:
                sb.Append("dynwp");
                break;
            case WallpaperType.Uploaded:
                sb.Append("usrwp");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), Type, null);
        }

        sb.Append("}");
        sb.Append("-");
        sb.Append("{");
        switch (Type)
        {
            case WallpaperType.Static:
                sb.Append(StaticWallpaperUrl);
                break;
            case WallpaperType.Dynamic:
                sb.Append(DynamicWallpaperName);
                break;
            case WallpaperType.Uploaded:
                sb.Append(MediaId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), Type, null);
        }

        sb.Append("}");
        return sb.ToString();
    }
}
