using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Provider;

/// <inheritdoc/>
public class AppUriProvider(IConfiguration configuration) : IAppUriProvider
{
    /// <inheritdoc/>
    public Uri GetAbsoluteUri(string relativeUri)
    {
        string? frontendHost = configuration["Frontend:Host"]?.TrimEnd('/');
        string? backendHost = configuration["Backend:Host"]?.TrimEnd('/');

        string host = "";
        if (!string.IsNullOrEmpty(backendHost)
            && Uri.IsWellFormedUriString(backendHost, UriKind.Absolute))
            host = backendHost;
        else
            host = $"{frontendHost}{backendHost}";


        // // Check if frontendHost is an absolute server host
        // string? mediaHost = null;
        // if (!string.IsNullOrEmpty(frontendHost)
        //     && Uri.IsWellFormedUriString(frontendHost, UriKind.Absolute))
        // {
        //     mediaHost = frontendHost;
        // }
        //
        // // Else: check if backendHost is an absolute server host
        // else if (!string.IsNullOrEmpty(backendHost)
        //          && Uri.IsWellFormedUriString(backendHost, UriKind.Absolute))
        // {
        //     mediaHost = backendHost;
        // }
        //
        // mediaHost = mediaHost?.TrimEnd('/');
        //
        // if (string.IsNullOrEmpty(mediaHost))
        // {
        //     throw new InvalidOperationException("No valid Frontend:Host or Backend:Host configuration found.");
        // }

        string absoluteUri = $"{host}{relativeUri}";
        Uri.TryCreate(absoluteUri, UriKind.Absolute, out Uri? mediaUri);

        return mediaUri ?? throw new InvalidOperationException($"Invalid URI: {absoluteUri}");
    }
}
