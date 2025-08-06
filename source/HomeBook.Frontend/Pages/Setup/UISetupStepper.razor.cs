using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HomeBook.Frontend.Pages.Setup;

public partial class UISetupStepper : ComponentBase
{
    /// <summary>
    ///
    /// </summary>
    [Parameter] public bool IsVertical { get; set; } = false;

    /// <summary>
    ///
    /// </summary>
    [Parameter] public int ActiveIndex { get; set; } = 1;

    private MudStepper _setupStepper;
}
