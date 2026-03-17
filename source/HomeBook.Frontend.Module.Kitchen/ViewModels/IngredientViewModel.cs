using System.Globalization;
using System.Text.RegularExpressions;

namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public class IngredientViewModel
{
    public Guid Id { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AdditionalText { get; set; }

    public string DisplayText =>
        (Quantity.HasValue ?  Quantity.Value.ToString(CultureInfo.InvariantCulture) + " " : "") +
        (Unit != null ? Unit + " " : "") +
        Name +
        (AdditionalText != null ? ", " + AdditionalText : "");
}
