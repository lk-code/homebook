using MudBlazor;
using MudBlazor.Utilities;

namespace HomeBook.Frontend.Themes;


public class HomebookTheme : MudTheme
{
    // https://www.color-hex.com/color-palette/1063036
    private static MudColor Primary { get; set; } = "#382960";
    private static MudColor Secondary { get; set; } = "#13bd85";
    private static MudColor Tertiary { get; set; } = "#373f31";
    private static MudColor TextPrimary { get; set; } = "#080606";
    private static MudColor TextSecondary { get; set; } = TextPrimary.ColorLighten(0.4);

    private static readonly MudTheme _theme = new()
    {
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "20px"
        },
        PaletteLight = new PaletteLight()
        {
            Primary = Primary,
            Secondary = Secondary,
            Tertiary = Tertiary,

            Background = "#F5F5F5",

            TextPrimary = TextPrimary,
            TextSecondary = TextSecondary,


            AppbarBackground = "#382960",
            AppbarText = "#000000",

            DrawerBackground = "#382960",
            DrawerText = "#FFFFFF"
        },
    };

    public static MudTheme GetTheme() => _theme;
}
