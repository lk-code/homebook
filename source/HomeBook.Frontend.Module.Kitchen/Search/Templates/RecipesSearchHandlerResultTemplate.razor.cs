using HomeBook.Frontend.Module.Kitchen.ViewModels;
using HomeBook.Frontend.Modules.Abstractions;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Module.Kitchen.Search.Templates;

public partial class RecipesSearchHandlerResultTemplate : ComponentBase, ISearchHandlerResultTemplate
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public IReadOnlyList<SearchHandlerResultTemplateItem> Items { get; set; } = [];

    private readonly List<RecipeSearchResultViewModel> _searchResults = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        _searchResults.Clear();
        foreach (SearchHandlerResultTemplateItem item in Items)
        {
            RecipeSearchResultViewModel vm = new()
            {
                Id = Guid.ParseExact(item.Identifier, "D"),
                Name = item.Title,
                Description = item.Description
            };

            List<Guid> recipeImages = await RecipeService
                .GetImagesByRecipeIdAsync(vm.Id,
                    CancellationToken.None);
            if (recipeImages.Any())
                vm.HeroMediaId = recipeImages.FirstOrDefault();

            if (vm.HeroMediaId.HasValue)
                vm.HeroImageUri = await MediaService
                    .GetUrlForMediaItemAsync(vm.HeroMediaId.Value,
                        CancellationToken.None);

            _searchResults.Add(vm);
        }

        StateHasChanged();
    }
}
