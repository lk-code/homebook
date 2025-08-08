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
    }

    public void Dispose()
    {
        SetupService.OnSetupStepsInitialized -= OnSetupStepsInitialized;
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

        _activeSetupStep = await SetupService.GetActiveSetupStepAsync(cancellationToken);

        await InvokeAsync(StateHasChanged);
    }

    private void Callback(MouseEventArgs obj)
    {
        // SetupService.SetIsDone(true);
        NavigationManager.NavigateTo("/");
    }
}
