using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Setup.SetupSteps;

namespace HomeBook.Frontend.Setup;

public class SetupService : ISetupService
{
    private int SERVICE_ID = new Random().Next(1000, 9999);
    private bool _isDone = false;
    private List<ISetupStep> _setupSteps = [];
    public Func<ISetupStep, Task>? OnStepSuccessful { get; set; }
    public Func<ISetupStep, bool, Task>? OnStepFailed { get; set; }
    public Func<Task>? OnSetupStepsInitialized { get; set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        List<ISetupStep> setupSteps = [];

        setupSteps.Add(new BackendConnectionSetupStep());

        bool hasDatabaseConnectionString = false;
        if (hasDatabaseConnectionString)
        {
            setupSteps.Add(new DatabaseConnectionSetupStep());
        }
        else
        {
            setupSteps.Add(new DatabaseFormSetupStep());
        }

        setupSteps.Add(new DatabaseMigrationSetupStep());
        setupSteps.Add(new AdminUserSetupStep());
        setupSteps.Add(new ConfigurationSetupStep());

        _setupSteps = setupSteps;
    }

    public async Task TriggerOnMudStepInitialized(CancellationToken cancellationToken = default)
    {
        if (OnSetupStepsInitialized is not null)
            await OnSetupStepsInitialized.Invoke();
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

    public async Task SetStepStatusAsync(bool success,
        bool hasError,
        CancellationToken cancellationToken = default)
    {
        ISetupStep? activeStep = await GetActiveSetupStepAsync(cancellationToken);
        if (activeStep is null)
            throw new InvalidOperationException("No active setup step found.");

        if (hasError)
        {
            // set as failed
            activeStep.HasError = true;

            if (OnStepFailed != null)
                await OnStepFailed.Invoke(activeStep, true);
            return;
        }

        // reset failed status
        activeStep.HasError = false;
        if (OnStepFailed != null)
            await OnStepFailed.Invoke(activeStep, false);

        if (success)
        {
            // set as successful
            activeStep.IsSuccessful = true;
            if (OnStepSuccessful != null)
                await OnStepSuccessful.Invoke(activeStep);
        }
    }
}
