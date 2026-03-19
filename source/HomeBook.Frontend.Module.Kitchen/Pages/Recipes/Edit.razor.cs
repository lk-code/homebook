using HomeBook.Frontend.Module.Kitchen.Mappings;
using HomeBook.Frontend.Module.Kitchen.Models;
using HomeBook.Frontend.Module.Kitchen.ViewModels;
using HomeBook.Frontend.Modules.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace HomeBook.Frontend.Module.Kitchen.Pages.Recipes;

public partial class Edit : ComponentBase
{
    [Parameter]
    public Guid RecipeId { get; set; }

    [Inject(Key = "HomeBook.Frontend.Module.Kitchen.Module")]
    public IModule ModuleInstance { get; set; } = default!;

    private bool _isUploadingImage = false;
    private string? _acceptedFileTypes = ".png, .jpg, .jpeg, .webp";
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
        CancellationToken cancellationToken = CancellationToken.None;
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
            _recipe.Source,
            _recipe.ImageMediaIds.ToList(),
            cancellationToken);

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

    private static int GetDurationHours(TimeSpan? duration) => duration.HasValue ? (int)duration.Value.TotalHours : 0;

    private static int GetDurationMinutes(TimeSpan? duration) => duration.HasValue ? duration.Value.Minutes : 0;

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

    private async Task UploadRecipeImagesAsync(InputFileChangeEventArgs args)
    {
        try
        {
            _isUploadingImage = true;
            StateHasChanged();

            string fileName = args.File.Name;
            using var stream = args.File.OpenReadStream((50 * 1024 * 1024));
            using var ms = new MemoryStream();

            await stream.CopyToAsync(ms);
            byte[] fileContent = ms.ToArray();


            // Upload file and save MediaId
            CancellationToken cancellationToken = CancellationToken.None;
            Guid? recipeImagesStorageScopeId = await FileStorageRegistration.GetScopeIdForModuleAsync(ModuleInstance,
                "RecipeImages",
                cancellationToken);
            if (recipeImagesStorageScopeId is null)
                return;
            Guid mediaItemId = await FileStorageService.WriteFileAllBytesAsync(recipeImagesStorageScopeId!.Value,
                fileName,
                fileContent,
                cancellationToken);
            Uri staticAssetUrl = await MediaService.GetUrlForMediaItemAsync(mediaItemId,
                cancellationToken);

            _recipe.ImageMediaIds.Add(mediaItemId);
            StateHasChanged();
        }
        catch (Exception err)
        {
        }
        finally
        {
            _isUploadingImage = false;
            StateHasChanged();
        }


        /*
        string fileName = "test.txt";
        string content = """
                         Hello World from the UI!

                         this is a test with a text!
                         """;
        byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);

        // 1. WRITE
        Guid mediaItemId = await FileStorageService.WriteFileAllBytesAsync(recipeImagesStorageScopeId!.Value, fileName, contentBytes, cancellationToken);
        // Guid mediaItemId = await FileStorageService.WriteFileAllTextAsync(scopeId, fileName, contentString, cancellationToken);

        // TODO: get static path for ui to get the file without auth
        // TODO: add caching for this endpoint
        Uri staticAssetUrl = await MediaService.GetUrlForMediaItemAsync(mediaItemId, cancellationToken);

        // 2. READ
        // byte[] responseContentBytes = await FileStorageService.GetFileAllBytesAsync(recipeImagesStorageScopeId!.Value, fileName, cancellationToken);
        // // string contentString = await FileStorageService.GetFileAllTextAsync(scopeId, fileName, cancellationToken);
        //
        // // 3. DELETE
        // await FileStorageService.DeleteFileAsync(recipeImagesStorageScopeId!.Value, fileName, cancellationToken);
        //
        // int i = 0;
        /* */
    }
}
