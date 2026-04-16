namespace HomeBook.Frontend.Abstractions.Contracts;

/// <summary>
/// Provides methods to display user-facing messages (error, info, success, warning).
/// </summary>
public interface IDisplayMessageService
{
    /// <summary>
    /// Displays an error message to the user.
    /// </summary>
    /// <param name="message">The error message text.</param>
    void ShowError(string message);

    /// <summary>
    /// Displays an informational message to the user.
    /// </summary>
    /// <param name="message">The informational message text.</param>
    void ShowInformation(string message);

    /// <summary>
    /// Displays a success message to the user.
    /// </summary>
    /// <param name="message">The success message text.</param>
    void ShowSuccess(string message);

    /// <summary>
    /// Displays a warning message to the user.
    /// </summary>
    /// <param name="message">The warning message text.</param>
    void ShowWarning(string message);
}

