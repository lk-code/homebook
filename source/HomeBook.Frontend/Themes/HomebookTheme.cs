using MudBlazor;

namespace HomeBook.Frontend.Themes;

public class HomebookTheme : MudTheme
{
    private static readonly MudTheme _theme = new()
    {
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "20px"
        },
        // https://www.color-hex.com/color-palette/1063036
        PaletteLight = new PaletteLight()
        {
            Primary = "#382960",
            TextPrimary = "#FFFFFF",

            Secondary = "#13bd85",
            TextSecondary = "#2a0606",

            Background = "#F5F5F5",

            AppbarBackground = "#382960",
            AppbarText = "#000000",

            DrawerBackground = "#382960",
            DrawerText = "#FFFFFF"
        },
    };

    public static MudTheme GetTheme() => _theme;
}
