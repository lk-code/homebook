using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.SetupSteps;

public class DatabaseConnectionSetupStep : ISetupStep
{
    public string Key { get; } = nameof(DatabaseConnectionSetupStep);
    public Task HandleStepAsync() => throw new NotImplementedException();
}
