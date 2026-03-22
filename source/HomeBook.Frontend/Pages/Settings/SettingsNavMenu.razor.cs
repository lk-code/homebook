using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Pages.Settings;

public partial class SettingsNavMenu : ComponentBase
{
    private bool _showDeveloperArea => DeveloperService.IsDevelopmentModeActive();
}
