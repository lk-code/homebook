using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Services;

/// <inheritdoc/>
public class MediaService(
    IAuthenticationService authenticationService,
    BackendClient backendClient,
    IAppUriProvider appUriProvider) : IMediaService
{
    /// <inheritdoc/>
    public async Task<Uri?> GetUrlForMediaItemAsync(Guid mediaItemId,
        CancellationToken cancellationToken)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);

        MediaUrlResponse? response = await backendClient.Media[mediaItemId]
            .Url
            .GetAsync(x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);

        if (response is null
            || string.IsNullOrEmpty(response.MediaUri))
            return null;

        Uri mediaUri = appUriProvider.GetAbsoluteUri(response.MediaUri);

        return mediaUri;
    }
}
