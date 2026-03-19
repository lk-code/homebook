using HomeBook.Frontend.Module.Kitchen.Mappings;
using HomeBook.Frontend.Module.Kitchen.Models;
using HomeBook.Frontend.Module.Kitchen.ViewModels;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Module.Kitchen.Pages.Recipes;

public partial class Overview : ComponentBase
{
    private List<RecipeViewModel> _recipes = [];
    private bool _isLoading = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        await LoadRecipesAsync();
    }

    private async Task LoadRecipesAsync()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        _isLoading = true;
        StateHasChanged();

        try
        {
            IEnumerable<RecipeDto> recipes = await RecipeService.GetRecipesAsync(string.Empty,
                cancellationToken);
            _recipes.Clear();
            foreach (RecipeDto recipeDto in recipes)
            {
                RecipeViewModel vm = recipeDto.ToViewModel();
                if (vm.HeroMediaId.HasValue)
                    vm.HeroImageUri = await MediaService
                        .GetUrlForMediaItemAsync(recipeDto.HeroMediaId!.Value,
                            cancellationToken);

                _recipes.Add(vm);
            }
        }
        catch (Exception)
        {
            // TODO: display error
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task CreateRecipeAsync()
    {
        string recipeName = "Cheeseburger";
        string description =
            "Ein leckerer Cheeseburger mit saftigem Rindfleisch, geschmolzenem Käse, frischem Salat, Tomaten und Zwiebeln, serviert in einem weichen Brötchen.";
        int servings = 2;
        var steps = new List<RecipeStepDto>();
        var ingredients = new List<RecipeIngredientDto>();
        int durationWorkingMinutes = 45;
        int durationCookingMinutes = 45;
        int durationRestingMinutes = 45;
        int caloriesKcal = 3250;
        string comments = "3250";
        string source = "3250";
        CancellationToken cancellationToken = CancellationToken.None;

        await RecipeService.CreateOrUpdateRecipeAsync(null,
            recipeName,
            description,
            servings,
            steps.ToArray(),
            ingredients.ToArray(),
            durationWorkingMinutes,
            durationCookingMinutes,
            durationRestingMinutes,
            caloriesKcal,
            comments,
            source,
            [],
            cancellationToken);
    }
}
