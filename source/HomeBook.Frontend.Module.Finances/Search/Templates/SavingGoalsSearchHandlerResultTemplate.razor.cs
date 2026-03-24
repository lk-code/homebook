using HomeBook.Frontend.Modules.Abstractions;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Module.Finances.Search.Templates;

public partial class SavingGoalsSearchHandlerResultTemplate : ComponentBase, ISearchHandlerResultTemplate
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public IReadOnlyList<SearchHandlerResultTemplateItem> Items { get; set; } = [];

    private static bool CanNavigate(SearchHandlerResultTemplateItem item) =>
        Guid.TryParse(item.Identifier, out _);

    private void NavigateToSavingGoal(SearchHandlerResultTemplateItem item)
    {
        if (!Guid.TryParse(item.Identifier, out Guid savingGoalId))
            return;

        NavigationManager.NavigateTo($"/Finances/Savings/{savingGoalId}");
    }
}
