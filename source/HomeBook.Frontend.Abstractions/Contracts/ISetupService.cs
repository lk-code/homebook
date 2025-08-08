namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupService
{
    Guid ServiceId { get; }
    Func<ISetupStep, Task>? OnStepSuccessful { get; set; }
    Func<ISetupStep, Task>? OnStepFailed { get; set; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<ISetupStep[]> GetSetupStepsAsync(CancellationToken cancellationToken = default);
    Task<ISetupStep?> GetActiveSetupStepAsync(CancellationToken cancellationToken = default);
    Task<bool> IsSetupDoneAsync(CancellationToken cancellationToken = default);
    Task FinishActiveStepAsync(bool success,
        CancellationToken cancellationToken = default);
}
