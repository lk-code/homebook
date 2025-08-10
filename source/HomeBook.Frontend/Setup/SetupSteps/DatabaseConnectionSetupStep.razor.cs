using HomeBook.Frontend.Abstractions.Contracts;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Setup.SetupSteps;

public partial class DatabaseConnectionSetupStep : ComponentBase, ISetupStep
{
    private int SERVICE_ID = new Random().Next(1000, 9999);

    public string Key { get; } = nameof(DatabaseConnectionSetupStep);
    public bool HasError { get; set; }
    public bool IsSuccessful { get; set; }
    public Task HandleStepAsync() => throw new NotImplementedException();

    public Task<bool> IsStepDoneAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    [Parameter]
    public string DatabaseName { get; set; } = string.Empty;
    [Parameter]
    public string DatabaseUserName { get; set; } = string.Empty;
    [Parameter]
    public string DatabaseUserPassword { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        await InvokeAsync(StateHasChanged);
    }
}
