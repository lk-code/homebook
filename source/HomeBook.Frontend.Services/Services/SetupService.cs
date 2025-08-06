using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Services.Services;

public class SetupService : ISetupService
{
    private bool _isDone = false;

    public Guid ServiceId { get; } = Guid.NewGuid();
    public Func<Task>? OnStepSuccessful { get; set; }
    public Func<Exception, Task>? OnStepFailed { get; set; }

    public async Task<bool> IsSetupDoneAsync(CancellationToken cancellationToken = default)
    {
        return false;
    }

    public async Task NextStepAsync(bool success, CancellationToken cancellationToken = default)
    {
        if (success)
        {
            if (OnStepSuccessful != null)
                await OnStepSuccessful.Invoke();
        }
        else
        {
            if (OnStepFailed != null)
                await OnStepFailed.Invoke(new Exception("Step failed"));
        }
    }
}
