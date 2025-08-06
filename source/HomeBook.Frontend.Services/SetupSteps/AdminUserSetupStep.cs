using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.SetupSteps;

public class AdminUserSetupStep : ISetupStep
{
    public string Key { get; } = nameof(AdminUserSetupStep);
    public Task HandleStepAsync() => throw new NotImplementedException();
}
