using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Modules.Abstractions;

namespace HomeBook.Frontend.Services;

public class MediaService(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : IMediaService
{
    public async Task<Uri> GetUrlForMediaItemAsync(Guid mediaItemId,
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

        Uri.TryCreate(response?.MediaUri, UriKind.Absolute, out Uri? mediaUri);

        return mediaUri;
    }
}
