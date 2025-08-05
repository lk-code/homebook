using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace HomeBook.Frontend.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthenticationStateProvider(HttpClient httpClient, ILogger<CustomAuthenticationStateProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return _anonymous;

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return _anonymous;
        }
    }

    public async Task LoginAsync(string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/account/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(result);

            if (tokenData.TryGetProperty("accessToken", out var tokenElement))
            {
                var token = tokenElement.GetString();
                await SetTokenAsync(token);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }
        else
        {
            throw new UnauthorizedAccessException("Login fehlgeschlagen");
        }
    }

    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("/account/logout", null);
        await RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    private async Task<string?> GetTokenAsync()
    {
        // In einer echten Anwendung würde man hier localStorage oder einen sicheren Speicher verwenden
        // Für diese Demo verwenden wir eine vereinfachte Implementierung
        return await Task.FromResult((string?)null);
    }

    private async Task SetTokenAsync(string? token)
    {
        // Token speichern (localStorage, secure storage, etc.)
        await Task.CompletedTask;
    }

    private async Task RemoveTokenAsync()
    {
        // Token entfernen
        await Task.CompletedTask;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out var roles);

            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                    claims.AddRange(parsedRoles!.Select(role => new Claim(ClaimTypes.Role, role)));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}
