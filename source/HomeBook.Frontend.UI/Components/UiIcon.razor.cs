using HomeBook.Frontend.UI.Utilities;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.UI.Components;

public partial class UiIcon : ComponentBase
{
    protected string ContainerCss =>
        new HtmlArgumentBuilder("ui-icon")
            .AddClass("ui-icon-filled", IconStyle == UiIconStyle.Filled)
            .AddClass("ui-icon-outlined", IconStyle == UiIconStyle.Outlined)
            .AddClass("ui-icon-plain", IconStyle == UiIconStyle.Plain)
            .AddClass(Class)
            .Build();

    protected string IconCss =>
        new HtmlArgumentBuilder("ui-icon-el")

            .AddClass("ui-icon-filled",
                IconStyle == UiIconStyle.Filled)
            .AddClass("",
                IconStyle == UiIconStyle.Filled
                && Color != Color.Default
                && Color != Color.Inherit)

            .AddClass("ui-icon-outlined",
                IconStyle == UiIconStyle.Outlined)
            .AddClass("",
                IconStyle == UiIconStyle.Outlined
                && Color != Color.Default
                && Color != Color.Inherit)

            .AddClass($"ui-icon-plain",
                IconStyle == UiIconStyle.Plain)
            .AddClass($"mud-{Color.ToString()}-text",
                IconStyle == UiIconStyle.Plain
                && Color != Color.Default
                && Color != Color.Inherit)

            .Build();

    protected string Style =>
        new HtmlArgumentBuilder("")

            /* UiIconStyle == Filled */
            .AddClass($"background-color: {ColorValue};",
                IconStyle == UiIconStyle.Filled
                && Color == Color.Default)
            .AddClass($"background-color: var(--mud-palette-{Color.ToString()});",
                IconStyle == UiIconStyle.Filled
                && Color != Color.Default
                && Color != Color.Inherit)
            .AddClass($"fill: var(--mud-palette-{Color.ToString()}-text);",
                IconStyle == UiIconStyle.Filled
                && Color != Color.Default
                && Color != Color.Inherit)

            /* UiIconStyle == Outlined */
            .AddClass($"border-color: {ColorValue};",
                IconStyle == UiIconStyle.Outlined
                && Color == Color.Default)
            .AddClass($"fill: {ColorValue};",
                IconStyle == UiIconStyle.Outlined
                && Color == Color.Default)
            .AddClass($"border-color: var(--mud-palette-{Color.ToString()});",
                IconStyle == UiIconStyle.Outlined
                && Color != Color.Default
                && Color != Color.Inherit)
            .AddClass($"fill: var(--mud-palette-{Color.ToString()});",
                IconStyle == UiIconStyle.Outlined
                && Color != Color.Default
                && Color != Color.Inherit)

            /* UiIconStyle == Plain */
            .AddClass($"fill: {ColorValue};",
                IconStyle == UiIconStyle.Plain
                && Color == Color.Default)

            .Build();

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string Icon { get; set; } = string.Empty;

    [Parameter]
    public Color Color { get; set; } = Color.Default;

    /// <summary>
    /// like '#ff0000', 'rgb(255,0,0)' or 'var(--my-color)';
    /// </summary>
    [Parameter]
    public string ColorValue { get; set; } = string.Empty;

    [Parameter]
    public UiIconStyle IconStyle { get; set; } = UiIconStyle.Plain;

    [Parameter]
    public Size Size { get; set; } = Size.Medium;
}

public enum UiIconStyle
{
    Filled,
    Outlined,
    Plain
}
