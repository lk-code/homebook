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
    private MudDropContainer<MediaItemViewModel>? _recipeImageDropContainer = null;
    private bool _nameEditMode = false;
    private bool _nameEditUpdate = false;
    private const string RecipeImagesDropZoneIdentifier = "recipe-images";

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
                return;
            }

            _recipe = recipeDto.ToViewModel();
            await LoadRecipeImagesAsync(recipeDto.ImageMediaItems,
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
            _recipe.ImageMediaItems
                .Select(x => new RecipeMediaItemDto(x.Id, x.Index))
                .ToArray(),
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

            CancellationToken cancellationToken = CancellationToken.None;
            Guid? recipeImagesStorageScopeId = await FileStorageRegistration.GetScopeIdForModuleAsync(ModuleInstance,
                "RecipeImages",
                cancellationToken);
            if (recipeImagesStorageScopeId is null)
                return;

            IReadOnlyList<IBrowserFile> files = args.GetMultipleFiles(10);
            foreach (IBrowserFile file in files)
            {
                using var stream = file.OpenReadStream((50 * 1024 * 1024));
                using var ms = new MemoryStream();

                await stream.CopyToAsync(ms);
                byte[] fileContent = ms.ToArray();

                Guid mediaItemId = await FileStorageService.WriteFileAllBytesAsync(recipeImagesStorageScopeId.Value,
                    file.Name,
                    fileContent,
                    cancellationToken);

                await AddRecipeImageAsync(mediaItemId, cancellationToken);
            }

            RefreshRecipeImageDropContainer();
            StateHasChanged();
        }
        catch (Exception)
        {
        }
        finally
        {
            _isUploadingImage = false;
            StateHasChanged();
        }
    }

    private async Task LoadRecipeImagesAsync(IEnumerable<RecipeMediaItemDto> mediaItems,
        CancellationToken cancellationToken)
    {
        if (_recipe is null)
            return;

        RecipeMediaItemDto[] orderedMediaItems = (mediaItems ?? [])
            .OrderBy(x => x.Index)
            .ToArray();
        if (orderedMediaItems.Length == 0)
        {
            _recipe.ImageMediaItems = [];
            NormalizeRecipeImageOrder();
            return;
        }

        List<MediaItemViewModel> imageMediaItems = [];
        foreach (RecipeMediaItemDto mediaItem in orderedMediaItems)
            imageMediaItems.Add(await BuildMediaItemViewModelAsync(mediaItem, cancellationToken));

        _recipe.ImageMediaItems = imageMediaItems;
        NormalizeRecipeImageOrder();
    }

    private async Task AddRecipeImageAsync(Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        if (_recipe is null)
            return;

        Uri absoluteUri = await MediaService.GetUrlForMediaItemAsync(mediaItemId,
            cancellationToken);
        _recipe.ImageMediaItems.Add(new MediaItemViewModel(mediaItemId,
            absoluteUri,
            _recipe.ImageMediaItems.Count));
        NormalizeRecipeImageOrder();
    }

    private async Task<MediaItemViewModel> BuildMediaItemViewModelAsync(RecipeMediaItemDto mediaItem,
        CancellationToken cancellationToken)
    {
        Uri absoluteUri = await MediaService.GetUrlForMediaItemAsync(mediaItem.MediaItemId,
            cancellationToken);

        return new MediaItemViewModel(mediaItem.MediaItemId,
            absoluteUri,
            mediaItem.Index);
    }

    private void RemoveRecipeImage(Guid mediaItemId)
    {
        if (_recipe is null)
            return;

        _recipe.ImageMediaItems.RemoveAll(x => x.Id == mediaItemId);
        NormalizeRecipeImageOrder();
        RefreshRecipeImageDropContainer();
    }

    private async Task OnRecipeImageDropped(MudItemDropInfo<MediaItemViewModel> dropInfo)
    {
        if (_recipe is null
            || dropInfo.Item is null)
            return;

        List<MediaItemViewModel> orderedMediaItems = _recipe.ImageMediaItems.ToList();
        int sourceIndex = orderedMediaItems.FindIndex(x => x.Id == dropInfo.Item.Id);
        if (sourceIndex < 0)
            return;

        MediaItemViewModel movedItem = orderedMediaItems[sourceIndex];
        orderedMediaItems.RemoveAt(sourceIndex);

        int targetIndex = Math.Clamp(dropInfo.IndexInZone,
            0,
            orderedMediaItems.Count);
        orderedMediaItems.Insert(targetIndex, movedItem);

        _recipe.ImageMediaItems = orderedMediaItems;
        StateHasChanged();

        NormalizeRecipeImageOrder();
        RefreshRecipeImageDropContainer();
        StateHasChanged();
    }

    private void NormalizeRecipeImageOrder()
    {
        if (_recipe is null)
            return;

        for (int i = 0; i < _recipe.ImageMediaItems.Count; i++)
            _recipe.ImageMediaItems[i].Index = i;

        _recipe.ImageMediaIds = _recipe.ImageMediaItems
            .Select(x => x.Id)
            .ToList();
        _recipe.HeroMediaId = _recipe.ImageMediaItems
            .Select(x => (Guid?)x.Id)
            .FirstOrDefault();
    }

    private void RefreshRecipeImageDropContainer()
    {
        _recipeImageDropContainer?.Refresh();
    }
}
