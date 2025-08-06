using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.SetupSteps;

public class ConfigurationSetupStep : ISetupStep
{
    public string Key { get; } = nameof(ConfigurationSetupStep);
    public Task HandleStepAsync() => throw new NotImplementedException();
}
