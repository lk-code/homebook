namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupService
{
    Func<ISetupStep, Task>? OnStepSuccessful { get; set; }
    Func<ISetupStep, bool, Task>? OnStepFailed { get; set; }
    Func<Task>? OnSetupStepsInitialized { get; set; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task TriggerOnMudStepInitialized(CancellationToken cancellationToken = default);
    Task<ISetupStep[]> GetSetupStepsAsync(CancellationToken cancellationToken = default);
    Task<ISetupStep?> GetActiveSetupStepAsync(CancellationToken cancellationToken = default);
    Task<bool> IsSetupDoneAsync(CancellationToken cancellationToken = default);
    Task SetStepStatusAsync(bool success, bool hasError, CancellationToken cancellationToken = default);
}
