using HomeBook.Frontend.Abstractions.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HomeBook.Frontend.Pages.Setup;

public partial class UISetupContent : ComponentBase, IDisposable
{
    private ISetupStep? _activeSetupStep = null;

    protected override async Task OnInitializedAsync()
    {
        SetupService.OnSetupStepsInitialized += OnSetupStepsInitialized;
        SetupService.OnStepSuccessful += OnStepSuccessful;
        SetupService.OnStepFailed += OnStepFailed;
    }

    public void Dispose()
    {
        SetupService.OnSetupStepsInitialized -= OnSetupStepsInitialized;
        SetupService.OnStepSuccessful -= OnStepSuccessful;
        SetupService.OnStepFailed -= OnStepFailed;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        // load settings
    }

    private async Task OnSetupStepsInitialized()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        await LoadActiveStepToUiAsync(cancellationToken);
    }

    private async Task OnStepFailed(ISetupStep arg)
    {
        // TODO: display error
    }

    private async Task OnStepSuccessful(ISetupStep arg)
    {
        CancellationToken cancellationToken = CancellationToken.None;

        await LoadActiveStepToUiAsync(cancellationToken);
    }

    private async Task LoadActiveStepToUiAsync(CancellationToken cancellationToken)
    {
        _activeSetupStep = await SetupService.GetActiveSetupStepAsync(cancellationToken);
        await InvokeAsync(StateHasChanged);
    }

    private void Callback(MouseEventArgs obj)
    {
        // SetupService.SetIsDone(true);
        NavigationManager.NavigateTo("/");
    }
}
