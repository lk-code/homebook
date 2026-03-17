using HomeBook.Frontend.Module.Finances.Mappings;
using HomeBook.Frontend.Module.Finances.Models;
using HomeBook.Frontend.Module.Finances.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Module.Finances.Components;

public partial class HbSavingGoalEditForm : ComponentBase
{
    [Parameter]
    public SavingGoalDto? SavingGoal { get; set; }

    [Parameter]
    public EventCallback<SavingGoalDto> OnSave { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    protected MudForm? Form;
    protected SavingGoalDetailViewModel DetailViewModel { get; private set; } = new();

    protected bool IsEditMode => SavingGoal is not null;

    protected override void OnInitialized()
    {
        DetailViewModel = SavingGoal is null
            ? new SavingGoalDetailViewModel()
            : SavingGoal.ToDetailViewModel();
    }

    protected async Task OnValidated()
    {
        if (Form is null)
            return;

        await Form.ValidateAsync();
    }

    protected async Task OnSaveClicked()
    {
        if (Form is null)
            return;

        await Form.ValidateAsync();

        if (!Form.IsValid)
            return;

        DetailViewModel.Id = (IsEditMode ? SavingGoal!.Id : null);
        SavingGoalDto dto = DetailViewModel.ToDto();
        await OnSave.InvokeAsync(dto);
    }

    protected async Task OnCancelClicked()
    {
        await OnCancel.InvokeAsync();
    }
}
