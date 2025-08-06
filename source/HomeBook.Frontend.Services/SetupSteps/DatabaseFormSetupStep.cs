using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.SetupSteps;

public class DatabaseFormSetupStep : ISetupStep
{
    public string Key { get; } = nameof(DatabaseFormSetupStep);
    public Task HandleStepAsync() => throw new NotImplementedException();
}
