using HomeBook.Frontend.Abstractions.Contracts;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Setup.SetupSteps;

public partial class DatabaseConnectionSetupStep : ComponentBase, ISetupStep
{
    public string Key { get; } = nameof(DatabaseConnectionSetupStep);
    public bool HasError { get; set; }
    public bool IsSuccessful { get; set; }
    public Task HandleStepAsync() => throw new NotImplementedException();

    public Task<bool> IsStepDoneAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    private string DatabaseName { get; set; } = string.Empty;
    private string DatabaseUserName { get; set; } = string.Empty;
    private string DatabaseUserPassword { get; set; } = string.Empty;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
            return;

        DatabaseName = SetupService.GetStoredConfigValue<string>("DatabaseName");
        DatabaseUserName = SetupService.GetStoredConfigValue<string>("DatabaseUserName");
        DatabaseUserPassword = SetupService.GetStoredConfigValue<string>("DatabaseUserPassword");
        await InvokeAsync(StateHasChanged);
    }
}
