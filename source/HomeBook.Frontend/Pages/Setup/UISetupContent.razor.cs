using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HomeBook.Frontend.Pages.Setup;

public partial class UISetupContent : ComponentBase
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        // load settings
    }

    private void Callback(MouseEventArgs obj)
    {
        // SetupService.SetIsDone(true);
        NavigationManager.NavigateTo("/");
    }
}
