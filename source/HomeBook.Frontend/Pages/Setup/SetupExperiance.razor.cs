using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Pages.Setup;

public partial class SetupExperiance : ComponentBase
{
    private string _appVersion = string.Empty;
    private string _appServer = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        _appVersion = Configuration["Version"] ?? "Unknown";
        _appServer = Configuration["Backend:Host"] ?? "Unknown";
        StateHasChanged();
    }
}
