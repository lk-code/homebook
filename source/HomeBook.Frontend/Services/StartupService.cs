using System.Globalization;
using System.Reflection;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Abstractions.Enums;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Services;

/// <inheritdoc />
public class StartupService(
    IServiceProvider serviceProvider,
    IInstanceManagementProvider instanceManagementProvider,
    IJsLocalStorageProvider jsLocalStorageProvider,
    ILocalizationService localizationService,
    ISetupService setupService,
    IDeveloperService developerService,
    ILogger<StartupService> logger) : IStartupService
{
    public event Action<AppStatus>? ApplicationInitialized;

    public AppStatus Status { get; private set; } = AppStatus.Initializing;
    private readonly HashSet<Assembly> _neededAssemblies = [];

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting application");

        // 1. wait for backend to be ready
        await setupService.InitializeAsync(cancellationToken);
        await WaitForServerAndGetSetupStatusAsync(cancellationToken);

        if (Status == AppStatus.Error)
        {
            // TODO: show error page with message that app could not started
            return;
        }

        // 2. startup application
        _neededAssemblies.Clear();

        if (Status == AppStatus.Running)
        {
            // load instance data into cache
            await LoadInstanceDataAsync(cancellationToken);
            await InitializeDeveloperServiceAsync(cancellationToken);
            await InitializeLocalizing(cancellationToken);
            await LoadingModuleAssemblies(cancellationToken);
        }

        // Notify authentication state changed
        ApplicationInitialized?.Invoke(Status);
    }

    private async Task InitializeDeveloperServiceAsync(CancellationToken cancellationToken)
    {
        await developerService.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Assembly[] GetRequiredAssemblies() => _neededAssemblies.ToArray();

    private async Task LoadingModuleAssemblies(CancellationToken cancellationToken)
    {
        IEnumerable<IModule> modules = serviceProvider.GetServices<IModule>();

        foreach (IModule module in modules)
        {
            Assembly moduleAssembly = module.GetType().Assembly;
            _neededAssemblies.Add(moduleAssembly);
        }
    }

    private async Task InitializeLocalizing(CancellationToken cancellationToken)
    {
        await localizationService.InitializeAsync(cancellationToken);

        string culture = await localizationService.GetCultureAsync(cancellationToken);
        await localizationService.SetCultureAsync(culture,
            false,
            cancellationToken);
    }

    private async Task WaitForServerAndGetSetupStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            Status = (await setupService.GetInstanceStatusAsync(cancellationToken)) ?? AppStatus.Error;
        }
        catch (HttpRequestException err) when (err.Message.ToLowerInvariant().Contains("failed to fetch"))
        {
            await WaitForServerAndGetSetupStatusAsync(cancellationToken);
        }
        catch (Exception)
        {
            logger.LogError("Failed to get instance status");
        }
    }

    private async Task LoadInstanceDataAsync(CancellationToken cancellationToken)
    {
        string instanceName = await instanceManagementProvider.GetInstanceNameAsync(cancellationToken);
        await jsLocalStorageProvider.SetItemAsync(JsLocalStorageKeys.HomeBookInstanceName,
            instanceName,
            cancellationToken);
    }
}
