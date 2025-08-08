using HomeBook.Frontend.Abstractions.Contracts;
using Microsoft.AspNetCore.Components;

namespace HomeBook.Frontend.Setup.SetupSteps;

public partial class BackendConnectionSetupStep : ComponentBase, ISetupStep
{
    private bool _isChecking = false;
    private bool _serverIsOk = false;
    private string? _errorMessage = null;

    public string Key { get; } = nameof(BackendConnectionSetupStep);
    public bool HasError { get; set; }
    public bool IsSuccessful { get; set; }
    public Task HandleStepAsync() => throw new NotImplementedException();

    public Task<bool> IsStepDoneAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    private async Task StartAsync()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        _isChecking = true;
        _serverIsOk = false;
        _errorMessage = null;
        await InvokeAsync(StateHasChanged);

        bool checkSuccessful = false;
        try
        {
            await Task.WhenAll(
                Task.Delay(5000, cancellationToken),
                ConnectToServerAsync(cancellationToken));

            _serverIsOk = true;
            checkSuccessful = true;
        }
        catch (HttpRequestException err)
        {
            // DE => Verbindung zum Server konnte nicht hergestellt werden. Stellen Sie sicher, dass der Server läuft und korrekt konfiguriert wurde und versuchen Sie es erneut.
            _errorMessage = "Unable to connect to the server. Make sure that the server is running and has been configured correctly, then try again.";
            await StepErrorAsync(cancellationToken);
        }
        catch (Exception err)
        {
            _errorMessage = "error while connecting to server: " + err.Message;
            await StepErrorAsync(cancellationToken);
        }
        finally
        {
            _isChecking = false;
            await InvokeAsync(StateHasChanged);
        }

        if (checkSuccessful)
        {
            await StepSuccessAsync(cancellationToken);
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ConnectToServerAsync(CancellationToken cancellationToken)
    {
        var version = await BackendClient
            .Weatherforecast
            .GetAsync((x) =>
            {
            }, cancellationToken);
    }

    private async Task StepErrorAsync(CancellationToken cancellationToken = default)
    {
        await SetupService.FinishActiveStepAsync(false, cancellationToken);
    }

    private async Task StepSuccessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(5000, cancellationToken);
        await SetupService.FinishActiveStepAsync(true, cancellationToken);
    }
}
