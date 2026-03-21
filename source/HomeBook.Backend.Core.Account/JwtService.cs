using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HomeBook.Backend.Core.Account;

/// <summary>
/// JWT service implementation for token operations
/// </summary>
public class JwtService(
    IConfiguration configuration,
    ILogger<JwtService> logger) : IJwtService
{
    private readonly string _secretKey = configuration["Jwt:SecretKey"]
                                         ?? throw new InvalidOperationException("JWT SecretKey is required");

    private readonly string _issuer = configuration["Jwt:Issuer"]
                                      ?? "HomeBook";

    private readonly string _audience = configuration["Jwt:Audience"]
                                        ?? "HomeBook";

    private readonly int _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"]
                                                        ?? "60");

    /// <inheritdoc />
    public JwtTokenResult GenerateToken(Guid userId, string username) => GenerateToken(userId, username, false);

    /// <inheritdoc />
    public JwtTokenResult GenerateToken(Guid userId, string username, bool isAdmin)
    {
        logger.LogInformation("Generating JWT token");

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_secretKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        ];

        if (isAdmin)
        {
            claims = claims.Append(new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "User")).ToArray();
            claims = claims.Append(new Claim("IsAdmin", isAdmin.ToString(), ClaimValueTypes.Boolean)).ToArray();
        }

        JwtSecurityToken token = new(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        JwtSecurityTokenHandler tokenHandler = new();
        string? tokenString = tokenHandler.WriteToken(token);
        string refreshToken = GenerateRefreshToken();

        return new JwtTokenResult
        {
            Token = tokenString,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserId = userId,
            Username = username
        };
    }

    /// <inheritdoc />
    public bool ValidateToken(string token)
    {
        try
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(_secretKey);

            tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "JWT token validation failed");
            return false;
        }
    }

    /// <inheritdoc />
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(_secretKey);

            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            Claim? userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract user ID from JWT token");
            return null;
        }
    }

    /// <inheritdoc />
    public bool IsAdminFromToken(string token)
    {
        try
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(_secretKey);

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            Claim? isAdminClaim = principal.FindFirst("IsAdmin");
            if (isAdminClaim != null && bool.TryParse(isAdminClaim.Value, out bool isAdmin))
            {
                return isAdmin;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract admin flag from JWT token");
            return false;
        }
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
