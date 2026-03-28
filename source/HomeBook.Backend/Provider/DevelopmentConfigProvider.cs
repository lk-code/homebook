using HomeBook.Backend.Abstractions.Contracts;

namespace HomeBook.Backend.Provider;

public class DevelopmentConfigProvider(
    ILogger<DevelopmentConfigProvider> logger,
    IConfiguration configuration) : IDevelopmentConfigProvider
{
    private const string MaskedValue = "**********";

    private static readonly HashSet<string> MaskedKeySegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "Secret",
        "ConnectionString",
    };

    private static readonly HashSet<string> MaskedKeyPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "Jwt:SecretKey",
    };

    public Task<List<KeyValuePair<string, string?>>> GetConfigurationValuesAsync(CancellationToken cancellationToken)
    {
        List<KeyValuePair<string, string?>> values = configuration
            .AsEnumerable()
            .Select(kvp => IsSensitive(kvp.Key)
                ? new KeyValuePair<string, string?>(kvp.Key, MaskedValue)
                : new KeyValuePair<string, string?>(kvp.Key, kvp.Value))
            .ToList();

        return Task.FromResult(values);
    }

    private static bool IsSensitive(string key)
    {
        foreach (string path in MaskedKeyPaths)
        {
            if (key.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                key.StartsWith(path + ":", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        foreach (string segment in key.Split(':'))
        {
            if (MaskedKeySegments.Contains(segment))
                return true;
        }

        return false;
    }
}

