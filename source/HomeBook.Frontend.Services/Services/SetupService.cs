using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Services.SetupSteps;

namespace HomeBook.Frontend.Services.Services;

public class SetupService : ISetupService
{
    private bool _isDone = false;
    private List<ISetupStep> _setupSteps = [];

    public Guid ServiceId { get; } = Guid.NewGuid();
    public Func<ISetupStep, Task>? OnStepSuccessful { get; set; }
    public Func<ISetupStep, Task>? OnStepFailed { get; set; }


    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        List<ISetupStep> setupSteps = [];

        setupSteps.Add(new BackendConnectionSetupStep());
        setupSteps.Add(new AdminUserSetupStep());

        bool hasDatabaseConnectionString = false;
        if (hasDatabaseConnectionString)
        {
            setupSteps.Add(new DatabaseConnectionSetupStep());
        }
        else
        {
            setupSteps.Add(new DatabaseFormSetupStep());
        }

        setupSteps.Add(new ConfigurationSetupStep());

        _setupSteps = setupSteps;
    }

    private async Task CheckSetupSteps(CancellationToken cancellationToken)
    {
        foreach (ISetupStep setupStep in _setupSteps)
        {
            bool isDone = await setupStep.IsStepDoneAsync(cancellationToken);

            if (isDone)
                await FinishActiveStepAsync(true,
                    cancellationToken);
        }
    }

    public async Task<ISetupStep[]> GetSetupStepsAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return _setupSteps.ToArray();
    }

    public async Task<ISetupStep?> GetActiveSetupStepAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        ISetupStep? activeStep = _setupSteps.FirstOrDefault(step => !step.IsSuccessful);

        return activeStep;
    }

    public async Task<bool> IsSetupDoneAsync(CancellationToken cancellationToken = default)
    {
        bool isSetupDone = _setupSteps.All(step => step.IsSuccessful);

        return isSetupDone;
    }

    public async Task FinishActiveStepAsync(bool success,
        CancellationToken cancellationToken = default)
    {
        ISetupStep? activeStep = await GetActiveSetupStepAsync(cancellationToken);
        if (activeStep is null)
            throw new InvalidOperationException("No active setup step found.");

        if (success)
        {
            activeStep.IsSuccessful = true;

            if (OnStepSuccessful != null)
                await OnStepSuccessful.Invoke(activeStep);
        }
        else
        {
            activeStep.HasError = true;

            if (OnStepFailed != null)
                await OnStepFailed.Invoke(activeStep);
        }
    }
}
