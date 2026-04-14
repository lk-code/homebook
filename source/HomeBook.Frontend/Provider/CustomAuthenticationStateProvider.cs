using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using HomeBook.Frontend.Abstractions.Contracts;

namespace HomeBook.Frontend.Provider;

/// <summary>
/// Custom authentication state provider for Blazor WASM that integrates with AuthenticationService
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthenticationService _authenticationService;

    public CustomAuthenticationStateProvider(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;

        // Subscribe to authentication state changes
        _authenticationService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            bool isAuthenticated = await _authenticationService.IsAuthenticatedAsync();

            if (!isAuthenticated)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            ClaimsPrincipal user = (await _authenticationService.GetCurrentUserAsync())!;
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication(ClaimsPrincipal user)
    {
        AuthenticationState authState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public void NotifyUserLogout()
    {
        ClaimsPrincipal anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        AuthenticationState authState = new AuthenticationState(anonymousUser);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    private async Task OnAuthenticationStateChanged(bool isAuthenticated,
        CancellationToken cancellationToken)
    {
        if (isAuthenticated)
        {
            // Trigger state refresh for login
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        else
        {
            // Notify logout
            NotifyUserLogout();
        }
    }

    public void Dispose()
    {
        _authenticationService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}
