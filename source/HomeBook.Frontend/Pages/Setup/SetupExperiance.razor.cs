using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HomeBook.Frontend.Pages.Setup;

public partial class SetupExperiance : ComponentBase
{
    private int _activeIndex = 1;

    private void Callback(MouseEventArgs obj)
    {
        SetupService.SetIsDone(true);
        NavigationManager.NavigateTo("/");
    }
}
