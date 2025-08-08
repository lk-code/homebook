using System.Collections.ObjectModel;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Models.Setup;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Pages.Setup;

public partial class UISetupStepper : ComponentBase, IAsyncDisposable
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

        CancellationToken cancellationToken = CancellationToken.None;

        if (!firstRender)
            return;

        SetupService.OnStepSuccessful += OnStepSuccessful;
        SetupService.OnStepFailed += OnStepFailed;

        ISetupStep[] setupSteps = await SetupService.GetSetupStepsAsync(cancellationToken);
        foreach (ISetupStep setupStep in setupSteps)
        {
            SetupStepViewModel stepVM = new(setupStep.Key, setupStep)
            {
                Completed = setupStep.IsSuccessful, HasError = setupStep.HasError
            };
            stepVM.OnUIDispatchRequired += OnUiDispatchRequired;
            _setupSteps.Add(stepVM);
        }

        _ = ObserveSetupStepVMsAsync(cancellationToken);
    }

    private async Task OnMudStepInitializedAsync()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        await LoadStepStatusAsync(cancellationToken);
    }

    private async Task ObserveSetupStepVMsAsync(CancellationToken cancellationToken)
    {
        List<SetupStepViewModel> stepsSnapshot = _setupSteps.ToList();
        List<Task> waitTasks = new(stepsSnapshot.Count);

        foreach (SetupStepViewModel vm in stepsSnapshot)
        {
            TaskCompletionSource<object?> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

            Func<MudStep?, Task>? handler = null;
            handler = (mudStep) =>
            {
                if (mudStep is not null)
                {
                    vm.OnStepRefSetAsync -= handler;
                    tcs.TrySetResult(null);
                }

                return Task.CompletedTask;
            };

            vm.OnStepRefSetAsync += handler;
            if (vm.StepRef is not null)
            {
                vm.OnStepRefSetAsync -= handler;
                tcs.TrySetResult(null);
            }

            waitTasks.Add(tcs.Task);
        }

        try
        {
            await Task
                .WhenAll(waitTasks)
                .WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await InvokeAsync(OnMudStepInitializedAsync);
    }

    public async ValueTask DisposeAsync()
    {
        SetupService.OnStepSuccessful -= OnStepSuccessful;
        SetupService.OnStepFailed -= OnStepFailed;

        foreach (SetupStepViewModel stepVM in _setupSteps)
        {
            stepVM.OnUIDispatchRequired -= OnUiDispatchRequired;
            await stepVM.DisposeAsync();
        }
    }

    private async Task OnUiDispatchRequired(Action arg)
    {
        await InvokeAsync(arg);
    }

    private async Task LoadStepStatusAsync(CancellationToken cancellationToken)
    {
        ISetupStep activeStatus = await SetupService.GetActiveSetupStepAsync(cancellationToken);
        SetupStepViewModel? stepVM = _setupSteps.FirstOrDefault(x => x.SetupStep.Key == activeStatus.Key);
        await SetActiveStepAsync(stepVM, cancellationToken);

        await InvokeAsync(StateHasChanged);
    }

    private async Task SetActiveStepAsync(SetupStepViewModel stepVM,
        CancellationToken cancellationToken = default)
    {
        if (_setupStepper is null
            || _setupStepper.ActiveStep == stepVM.StepRef
            || !_setupStepper.CanGoToNextStep)
            return;

        await _setupStepper.NextStepAsync();

        await SetActiveStepAsync(stepVM, cancellationToken);
    }

    private async Task OnStepFailed(ISetupStep step)
    {
        var stepVM = _setupSteps.FirstOrDefault(x => x.SetupStep.Key == step.Key);
        await stepVM.StepRef.SetHasErrorAsync(true, true);
        // await _setupStepper.ActiveStep.SetHasErrorAsync(true, true);
        StateHasChanged();
    }

    private async Task OnStepSuccessful(ISetupStep step)
    {
        var stepVM = _setupSteps.FirstOrDefault(x => x.SetupStep.Key == step.Key);
        stepVM.StepRef.Completed = true;
        // _setupStepper.ActiveStep.Completed = true;
        StateHasChanged();

        await _setupStepper.NextStepAsync();
    }
}
