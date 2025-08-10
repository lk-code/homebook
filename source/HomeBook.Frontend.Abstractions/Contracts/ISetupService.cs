namespace HomeBook.Frontend.Abstractions.Contracts;

public interface ISetupService
{
    Func<ISetupStep, Task>? OnStepSuccessful { get; set; }
    Func<ISetupStep, bool, Task>? OnStepFailed { get; set; }
    Func<Task>? OnSetupStepsInitialized { get; set; }

    /// <summary>
    /// initialize the setup service (load setup steps, etc.)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// triggers by ui component which handles the MudStepper component
    /// to notify the service that the MudStep component is initialized.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task TriggerOnMudStepInitialized(CancellationToken cancellationToken = default);

    Task<ISetupStep[]> GetSetupStepsAsync(CancellationToken cancellationToken = default);

    Task<ISetupStep?> GetActiveSetupStepAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// returns true if the setup is done, otherwise false.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsSetupDoneAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// set the status of the current step.
    /// </summary>
    /// <param name="success"></param>
    /// <param name="hasError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetStepStatusAsync(bool success, bool hasError, CancellationToken cancellationToken = default);

    /// <summary>
    /// returns a stored configuration value by key.
    /// If the value is not found, it will return the default value of the type.
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetStoredConfigValue<T>(string key);
}
