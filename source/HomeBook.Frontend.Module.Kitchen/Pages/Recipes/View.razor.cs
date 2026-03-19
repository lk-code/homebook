using HomeBook.Frontend.Module.Kitchen.Mappings;
using HomeBook.Frontend.Module.Kitchen.Models;
using HomeBook.Frontend.Module.Kitchen.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Module.Kitchen.Pages.Recipes;

public partial class View : ComponentBase
{
    [Parameter]
    public Guid RecipeId { get; set; }

    private bool _isLoading = false;
    private RecipeDetailViewModel? _recipeVM = null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        await LoadRecipeAsync();
    }

    private async Task LoadRecipeAsync()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        _isLoading = true;
        StateHasChanged();

        try
        {
            RecipeDetailDto? recipeDto = await RecipeService.GetRecipeByIdAsync(RecipeId,
                cancellationToken);
            if (recipeDto is null)
            {
                // recipe not found
                Snackbar.Add("+Recipe could not be found.", Severity.Error);
                NavigationManager.NavigateTo("/Kitchen/Recipes");
            }

            _recipeVM = recipeDto.ToViewModel();
            if (_recipeVM.HeroMediaId.HasValue)
                _recipeVM.HeroImageUri = await MediaService
                    .GetUrlForMediaItemAsync(recipeDto.HeroMediaId!.Value,
                        cancellationToken);
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

    private async Task DeleteRecipe()
    {
        try
        {
            await RecipeService.DeleteRecipeAsync(RecipeId);
            Snackbar.Add("+Recipe deleted successfully.", Severity.Success);

            NavigationManager.NavigateTo("/Kitchen/Recipes");
        }
        catch (Exception err)
        {
            Snackbar.Add("+Recipe could not be deleted. " + err.Message, Severity.Error);
        }
    }
}
