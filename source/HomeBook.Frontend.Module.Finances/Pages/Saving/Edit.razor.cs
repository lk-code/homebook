using HomeBook.Frontend.Module.Finances.Models;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Module.Finances.Pages.Saving;

public partial class Edit : ComponentBase
{
    [Parameter]
    public Guid SavingGoalId { get; set; }

    private bool _isLoading = false;
    private SavingGoalDto? _savingGoal = null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        CancellationToken cancellationToken = CancellationToken.None;
        await LoadSavingGoalAsync(cancellationToken);
    }

    private async Task LoadSavingGoalAsync(CancellationToken cancellationToken)
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            _savingGoal = await SavingGoalService.GetSavingGoalByIdAsync(SavingGoalId,
                cancellationToken);
            StateHasChanged();
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

    private void OnSavingGoalSave(SavingGoalDto dto)
    {
        // TODO: save thge goal data
    }
}
