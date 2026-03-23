using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Provider;

/// <inheritdoc/>
public class AppUriProvider(IConfiguration configuration) : IAppUriProvider
{
    /// <inheritdoc/>
    public Uri GetAbsoluteUri(string relativeUri)
    {
        string? frontendHost = configuration["Frontend:Host"];
        string? backendHost = configuration["Backend:Host"];

        // Check if frontendHost is an absolute server host
        string? mediaHost = null;
        if (!string.IsNullOrEmpty(frontendHost) && Uri.IsWellFormedUriString(frontendHost, UriKind.Absolute))
        {
            mediaHost = frontendHost;
        }
        // Else: check if backendHost is an absolute server host
        else if (!string.IsNullOrEmpty(backendHost) && Uri.IsWellFormedUriString(backendHost, UriKind.Absolute))
        {
            mediaHost = backendHost;
        }

        // TODO: remove ending slashes
        mediaHost = mediaHost?.TrimEnd('/');

        if (string.IsNullOrEmpty(mediaHost))
        {
            throw new InvalidOperationException("No valid Frontend:Host or Backend:Host configuration found.");
        }

        string absoluteUri = $"{mediaHost}{relativeUri}";
        Uri.TryCreate(absoluteUri, UriKind.Absolute, out Uri? mediaUri);

        return mediaUri ?? throw new InvalidOperationException($"Invalid URI: {absoluteUri}");
    }
}
