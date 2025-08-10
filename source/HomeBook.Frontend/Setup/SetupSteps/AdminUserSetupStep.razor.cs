using HomeBook.Frontend.Abstractions.Contracts;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Setup.SetupSteps;

public partial class AdminUserSetupStep : ComponentBase, ISetupStep
{
    public string Key { get; } = nameof(AdminUserSetupStep);
    public bool HasError { get; set; }
    public bool IsSuccessful { get; set; }
    public Task HandleStepAsync() => throw new NotImplementedException();

    public Task<bool> IsStepDoneAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}
