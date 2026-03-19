using HomeBook.Frontend.Module.Kitchen.ViewModels;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Module.Kitchen.Pages.Recipes.Components;

public partial class UiRecipeIngredientsList : ComponentBase
{
    [Parameter]
    public List<IngredientViewModel> Ingredients { get; set; } = [];

    [Parameter]
    public EventCallback<List<IngredientViewModel>> IngredientsChanged { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    private decimal? _newQuantity = null;
    private string? _newUnit = null;
    private string _newName = string.Empty;
    private string? _newAdditionalText = null;

    protected override void OnParametersSet()
    {
        if (Ingredients is null)
            Ingredients = [];
    }

    private async Task AddIngredient()
    {
        if (string.IsNullOrWhiteSpace(_newName))
            return;

        Ingredients.Add(new IngredientViewModel
        {
            Name = _newName.Trim(),
            Quantity = _newQuantity,
            Unit = string.IsNullOrWhiteSpace(_newUnit) ? null : _newUnit.Trim(),
            AdditionalText = string.IsNullOrWhiteSpace(_newAdditionalText) ? null : _newAdditionalText.Trim()
        });

        _newName = string.Empty;
        _newQuantity = null;
        _newUnit = null;
        _newAdditionalText = null;

        await IngredientsChanged.InvokeAsync(Ingredients);
    }

    private async Task RemoveIngredient(IngredientViewModel ingredient)
    {
        if (Ingredients.Remove(ingredient))
            await IngredientsChanged.InvokeAsync(Ingredients);
    }
}
