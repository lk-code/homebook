using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Pages.Settings;

public partial class Overview : ComponentBase
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo("/Settings/About");
    }
}
