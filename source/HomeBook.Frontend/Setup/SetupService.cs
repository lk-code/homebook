using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Setup.SetupSteps;

namespace HomeBook.Frontend.Setup;

public class SetupService(BackendClient backendClient) : ISetupService
{
    private bool _isDone = false;
    private List<ISetupStep> _setupSteps = [];
    public Func<ISetupStep, Task>? OnStepSuccessful { get; set; }
    public Func<ISetupStep, bool, Task>? OnStepFailed { get; set; }
    public Func<Task>? OnSetupStepsInitialized { get; set; }
    private Dictionary<string, object?> _storedConfigValues = new();

    public T GetStoredConfigValue<T>(string key)
    {
        if (_storedConfigValues.TryGetValue(key, out var value)
            && value is T typedValue)
        {
            return typedValue;
        }

        throw new KeyNotFoundException($"No stored config value found for key: {key}");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        List<ISetupStep> setupSteps = [];

        setupSteps.Add(new BackendConnectionSetupStep());
        await CheckDatabaseAndAddStep(setupSteps, cancellationToken);
        setupSteps.Add(new DatabaseMigrationSetupStep());
        setupSteps.Add(new AdminUserSetupStep());
        setupSteps.Add(new ConfigurationSetupStep());

        _setupSteps = setupSteps;
    }

    private async Task CheckDatabaseAndAddStep(List<ISetupStep> setupSteps, CancellationToken cancellationToken)
    {
        try
        {
            GetDatabaseCheckResponse? databaseCheckResponse = await backendClient.Setup.Check.Database.GetAsync(x =>
            {
            }, cancellationToken);

            if (databaseCheckResponse is not null)
            {
                // database configuration is set in environment variables
                // => load into form and check connection
                setupSteps.Add(new DatabaseConnectionSetupStep());

                // Store parameters separately
                _storedConfigValues["DatabaseName"] = databaseCheckResponse.DatabaseName;
                _storedConfigValues["DatabaseUserName"] = databaseCheckResponse.DatabaseUserName;
                _storedConfigValues["DatabaseUserPassword"] = databaseCheckResponse.DatabaseUserPassword;

                return;
            }
        }
        catch (Exception ex)
        {
            // no config found or error while checking
            // => do nothing and add form step after try catch
        }

        // no database configuration found in environment variables
        //   OR
        // error while checking database configuration
        // => show form to enter database configuration
        setupSteps.Add(new DatabaseFormSetupStep());
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
