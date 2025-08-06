using System.Collections.ObjectModel;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Models.Setup;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Pages.Setup;

public partial class UISetupStepper : ComponentBase, IDisposable
{
    /// <summary>
    ///
    /// </summary>
    [Parameter]
    public bool IsVertical { get; set; } = false;

    private MudStepper? _setupStepper = null;
    private ObservableCollection<SetupStepViewModel> _setupSteps = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        SetupService.OnStepSuccessful += OnStepSuccessful;
        SetupService.OnStepFailed += OnStepFailed;

        ISetupStep[] setupSteps = await SetupService.GetSetupStepsAsync();
        foreach (ISetupStep setupStep in setupSteps)
        {
            _setupSteps.Add(new SetupStepViewModel(setupStep.Key, setupStep));
        }
    }

    public void Dispose()
    {
        SetupService.OnStepSuccessful -= OnStepSuccessful;
        SetupService.OnStepFailed -= OnStepFailed;
    }

    private async Task OnStepFailed(Exception arg)
    {
        await _setupStepper.ActiveStep.SetHasErrorAsync(true, true);
        StateHasChanged();
    }

    private async Task OnStepSuccessful()
    {
        _setupStepper.ActiveStep.Completed = true;
        StateHasChanged();
        await _setupStepper.NextStepAsync();
    }
}
