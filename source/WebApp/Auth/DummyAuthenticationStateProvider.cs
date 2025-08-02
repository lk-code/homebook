using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace de.keysoftware.HomeBook.WebApp.Auth
{
    public class DummyAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Noch nicht eingeloggt: immer anonym
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}

