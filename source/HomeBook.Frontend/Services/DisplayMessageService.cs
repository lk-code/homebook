using HomeBook.Frontend.Abstractions.Contracts;
using MudBlazor;

namespace HomeBook.Frontend.Services;

/// <inheritdoc />
public sealed class DisplayMessageService(ISnackbar snackbar) : IDisplayMessageService
{
    private const int DefaultVisibleDurationMs = 5000;

    /// <inheritdoc />
    public void ShowError(string message)
    {
        snackbar.Add(message, Severity.Error, options =>
        {
            options.VisibleStateDuration = DefaultVisibleDurationMs;
            options.ShowCloseIcon = true;
        });
    }

    /// <inheritdoc />
    public void ShowInformation(string message)
    {
        snackbar.Add(message, Severity.Info, options =>
        {
            options.VisibleStateDuration = DefaultVisibleDurationMs;
            options.ShowCloseIcon = true;
        });
    }

    /// <inheritdoc />
    public void ShowSuccess(string message)
    {
        snackbar.Add(message, Severity.Success, options =>
        {
            options.VisibleStateDuration = DefaultVisibleDurationMs;
            options.ShowCloseIcon = true;
        });
    }

    /// <inheritdoc />
    public void ShowWarning(string message)
    {
        snackbar.Add(message, Severity.Warning, options =>
        {
            options.VisibleStateDuration = DefaultVisibleDurationMs;
            options.ShowCloseIcon = true;
        });
    }
}

