namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupService
{
    Guid ServiceId { get; }
    Func<Task>? OnStepSuccessful { get; set; }
    Func<Exception, Task>? OnStepFailed { get; set; }
    Task<ISetupStep[]> GetSetupStepsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsSetupDoneAsync(CancellationToken cancellationToken = default);
    Task NextStepAsync(bool success, CancellationToken cancellationToken = default);
}
