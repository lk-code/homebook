using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.SetupSteps;

public class DatabaseFormSetupStep : ISetupStep
{
    public string Key { get; } = nameof(DatabaseFormSetupStep);
    public bool HasError { get; set; }
    public bool IsSuccessful { get; set; }
    public async Task HandleStepAsync() => throw new NotImplementedException();
    public async Task<bool> IsStepDoneAsync(CancellationToken cancellationToken) => false;
}
