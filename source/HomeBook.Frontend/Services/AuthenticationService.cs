using System.Security.Claims;
using System.Text.Json;
using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using Microsoft.JSInterop;
using Microsoft.Kiota.Abstractions;

namespace HomeBook.Frontend.Services;

/// <inheritdoc />
public class AuthenticationService(
    ILogger<AuthenticationService> logger,
    BackendClient backendClient,
    IJSRuntime jsRuntime) : IAuthenticationService
{
    private const string CLAIMS_ID_KEY = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    private const string CLAIMS_NAME_KEY = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

    private const string TOKEN_KEY = "authToken";
    private const string REFRESH_TOKEN_KEY = "refreshToken";
    private const string EXPIRES_AT_KEY = "expiresAt";

    /// <inheritdoc />
    public event AsyncPreferenceChangedHandler<bool>? AuthenticationStateChanged;

    /// <inheritdoc />
    public async Task<bool> LoginAsync(string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Call backend login endpoint
            LoginResponse? response = await backendClient.Account.Login.PostAsync(new LoginRequest
                {
                    Username = username,
                    Password = password
                },
                x =>
                {
                },
                cancellationToken);

            if (response is null)
                return false;

            // Store tokens securely in localStorage
            await jsRuntime.InvokeVoidAsync("localStorage.setItem",
                cancellationToken,
                TOKEN_KEY,
                response.Token);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem",
                cancellationToken,
                REFRESH_TOKEN_KEY,
                response.RefreshToken);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem",
                cancellationToken,
                EXPIRES_AT_KEY,
                response.ExpiresAt!.Value.DateTime.ToString("O"));

            // Notify authentication state changed
            await InvokeAsync(AuthenticationStateChanged, true, cancellationToken);

            return true;
        }
        catch (ApiException err) when (err.ResponseStatusCode == 400)
        {
            logger.LogWarning("Invalid login attempt for user: {Username}", username);

            return false;
        }
        catch (ApiException err) when (err.ResponseStatusCode == 401)
        {
            logger.LogWarning("Unauthorized login attempt for user: {Username}", username);

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for user: {Username}", username);

            return false;
        }
    }

    /// <inheritdoc />
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string? token = await GetTokenAsync(cancellationToken);
            if (!string.IsNullOrEmpty(token))
            {
                // Call backend logout endpoint with Authorization header (use request-specific header instead of default)
                // TODO: add jwt token to kiota
                // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string? response = await backendClient.Account.Logout.PostAsync(
                    x =>
                    {
                    },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during backend logout call");
        }
        finally
        {
            // Clear stored tokens regardless of backend call result
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, TOKEN_KEY);
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, REFRESH_TOKEN_KEY);
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, EXPIRES_AT_KEY);

            logger.LogInformation("User logged out successfully");

            // Notify authentication state changed
            await InvokeAsync(AuthenticationStateChanged, false, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        string? token = await GetTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
            return false;

        // Check if token is expired
        string? expiresAtString = await jsRuntime.InvokeAsync<string?>("localStorage.getItem",
            cancellationToken,
            EXPIRES_AT_KEY);

        if (string.IsNullOrEmpty(expiresAtString)
            || !DateTime.TryParse(expiresAtString, out DateTime expiresAt))
            return false;

        if (DateTime.UtcNow < expiresAt)
            return true;

        logger.LogInformation("Token expired, logging out");
        await LogoutAsync(cancellationToken);
        return false;
    }

    /// <inheritdoc />
    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, TOKEN_KEY);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving token from localStorage");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ClaimsPrincipal?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsAuthenticatedAsync(cancellationToken))
            return null;

        string? token = await GetTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            // Parse JWT token to extract claims
            string[] tokenParts = token.Split('.');
            if (tokenParts.Length != 3)
                throw new FormatException("Invalid JWT token format");

            // Decode payload
            string payload = tokenParts[1];
            // Add padding if necessary
            while (payload.Length % 4 != 0)
            {
                payload += "=";
            }

            byte[] payloadBytes = Convert.FromBase64String(payload);
            string payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

            JsonDocument jsonDocument = JsonDocument.Parse(payloadJson);

            List<Claim> claims = new();

            // Extract standard claims
            if (jsonDocument.RootElement.TryGetProperty(CLAIMS_ID_KEY, out JsonElement subElement))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subElement.GetString() ?? ""));

            if (jsonDocument.RootElement.TryGetProperty(CLAIMS_NAME_KEY, out JsonElement nameElement))
                claims.Add(new Claim(ClaimTypes.Name, nameElement.GetString() ?? ""));

            // Extract admin claim
            if (jsonDocument.RootElement.TryGetProperty("IsAdmin", out JsonElement isAdminElement))
                claims.Add(new Claim("IsAdmin", isAdminElement.GetBoolean().ToString() ?? "false"));

            // Extract role claim
            if (jsonDocument.RootElement.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    out JsonElement roleElement))
                claims.Add(new Claim(ClaimTypes.Role, roleElement.GetString() ?? "User"));

            ClaimsIdentity identity = new(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing JWT token");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsCurrentUserAdminAsync(CancellationToken cancellationToken = default)
    {
        ClaimsPrincipal? user = await GetCurrentUserAsync(cancellationToken);
        Claim? isAdminClaim = user?.FindFirst("IsAdmin");

        if (isAdminClaim is not null
            && bool.TryParse(isAdminClaim.Value, out bool isAdmin))
            return isAdmin;

        return false;
    }

    /// <inheritdoc />
    public async Task IsAdminOrThrowAsync(CancellationToken cancellationToken = default)
    {
        bool isUserAdmin = await IsCurrentUserAdminAsync(cancellationToken);
        if (!isUserAdmin)
            throw new UnauthorizedAccessException("User is not authorized to access system information.");
    }

    private static Task InvokeAsync<T>(AsyncPreferenceChangedHandler<T>? eventHandler,
        T value,
        CancellationToken cancellationToken)
    {
        if (eventHandler is null)
            return Task.CompletedTask;

        Delegate[] delegates = eventHandler.GetInvocationList();
        Task[] tasks = new Task[delegates.Length];

        for (int i = 0; i < delegates.Length; i++)
            tasks[i] = ((AsyncPreferenceChangedHandler<T>)delegates[i]).Invoke(value, cancellationToken);

        return Task.WhenAll(tasks);
    }
}
