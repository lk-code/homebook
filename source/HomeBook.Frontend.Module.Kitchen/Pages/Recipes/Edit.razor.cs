using HomeBook.Frontend.Module.Kitchen.Mappings;
using HomeBook.Frontend.Module.Kitchen.Models;
using HomeBook.Frontend.Module.Kitchen.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Module.Kitchen.Pages.Recipes;

public partial class Edit : ComponentBase
{
    [Parameter]
    public Guid RecipeId { get; set; }

    private bool _isLoading = false;
    private RecipeDetailViewModel? _recipe = null;
    private bool _nameEditMode = false;
    private bool _nameEditUpdate = false;

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
            if (RecipeId == Guid.Empty)
            {
                _recipe = new RecipeDetailViewModel
                {
                    Name = string.Empty,
                    Description = string.Empty,
                    NumberOfServings = 1
                };
                _nameEditMode = true;
                return;
            }

            RecipeDetailDto? recipeDto = await RecipeService.GetRecipeByIdAsync(RecipeId,
                cancellationToken);
            if (recipeDto is null)
            {
                // recipe not found
                Snackbar.Add("+Recipe could not be found.", Severity.Error);
                NavigationManager.NavigateTo("/Kitchen/Recipes");
            }

            _recipe = recipeDto.ToViewModel();
            int i = 0;
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

    private async Task SaveRecipeAsync()
    {
        Guid? recipeId = RecipeId == Guid.Empty ? null : RecipeId;
        await RecipeService.CreateOrUpdateRecipeAsync(recipeId,
            _recipe!.Name,
            _recipe.Description,
            _recipe.NumberOfServings,
            _recipe.Steps?.Select((s, i) => s.ToDto(i)).ToArray(),
            _recipe.Ingredients?.Select(i => i.ToDto()).ToArray(),
            ToMinutes(_recipe.DurationWorkingMinutes),
            ToMinutes(_recipe.DurationCookingMinutes),
            ToMinutes(_recipe.DurationRestingMinutes),
            _recipe.CaloriesKcal,
            _recipe.Comments,
            _recipe.Source);

        if (recipeId is null)
            NavigationManager.NavigateTo("/Kitchen/Recipes");
    }

    private void AbortEditingRecipe()
    {
        NavigationManager.NavigateTo("/Kitchen/Recipes");
    }

    private async Task EditRecipeNameAsync()
    {
        _nameEditMode = true;
        StateHasChanged();
    }

    private async Task UpdateRecipeNameAsync()
    {
        try
        {
            _nameEditMode = false;
            _nameEditUpdate = true;
            StateHasChanged();

            CancellationToken cancellationToken = CancellationToken.None;
            string newName = _recipe?.Name ?? string.Empty;
            await RecipeService.UpdateRecipeNameAsync(RecipeId,
                newName,
                cancellationToken);

            await Task.Delay(5000, cancellationToken); // simulate delay
        }
        catch (Exception err)
        {
            Snackbar.Add("+Recipe name could not be updated. " + err.Message,
                Severity.Error);
        }
        finally
        {
            _nameEditUpdate = false;
            StateHasChanged();
        }
    }

    private static int GetDurationHours(TimeSpan? duration) =>
        duration.HasValue ? (int)duration.Value.TotalHours : 0;

    private static int GetDurationMinutes(TimeSpan? duration) =>
        duration.HasValue ? duration.Value.Minutes : 0;

    private void SetWorkingHours(int hours)
    {
        _recipe!.DurationWorkingMinutes = BuildDuration(hours, GetDurationMinutes(_recipe.DurationWorkingMinutes));
    }

    private void SetWorkingMinutes(int minutes)
    {
        _recipe!.DurationWorkingMinutes = BuildDuration(GetDurationHours(_recipe.DurationWorkingMinutes), minutes);
    }

    private void SetCookingHours(int hours)
    {
        _recipe!.DurationCookingMinutes = BuildDuration(hours, GetDurationMinutes(_recipe.DurationCookingMinutes));
    }

    private void SetCookingMinutes(int minutes)
    {
        _recipe!.DurationCookingMinutes = BuildDuration(GetDurationHours(_recipe.DurationCookingMinutes), minutes);
    }

    private void SetRestingHours(int hours)
    {
        _recipe!.DurationRestingMinutes = BuildDuration(hours, GetDurationMinutes(_recipe.DurationRestingMinutes));
    }

    private void SetRestingMinutes(int minutes)
    {
        _recipe!.DurationRestingMinutes = BuildDuration(GetDurationHours(_recipe.DurationRestingMinutes), minutes);
    }

    private static TimeSpan? BuildDuration(int hours,
        int minutes)
    {
        int safeHours = Math.Max(0, hours);
        int safeMinutes = Math.Max(0, minutes);
        if (safeHours == 0 && safeMinutes == 0)
            return null;

        return new TimeSpan(safeHours, safeMinutes, 0);
    }

    private static int? ToMinutes(TimeSpan? duration)
    {
        if (duration is null)
            return null;

        return (int)Math.Round(duration.Value.TotalMinutes);
    }
}
