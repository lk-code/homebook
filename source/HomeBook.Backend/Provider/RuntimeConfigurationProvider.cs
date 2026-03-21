using System.Text.Json;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Contracts;

namespace HomeBook.Backend.Provider;

public class RuntimeConfigurationProvider(
    ILogger<RuntimeConfigurationProvider> logger,
    IApplicationPathProvider applicationPathProvider,
    IFileSystemService fileSystemService) : IRuntimeConfigurationProvider
{
    /// <inheritdoc />
    public async Task UpdateConfigurationAsync(string key,
        object value,
        CancellationToken cancellationToken = default)
    {
        string appSettingsPath = applicationPathProvider.RuntimeConfigurationFilePath;

        // Read existing JSON or create empty object if file doesn't exist
        string fullAppsettingsJson = "{}";
        if (fileSystemService.FileExists(appSettingsPath))
            fullAppsettingsJson = await fileSystemService.FileReadAllTextAsync(appSettingsPath, cancellationToken);

        // Parse JSON
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(fullAppsettingsJson);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse runtime configuration");
            throw new InvalidOperationException($"Invalid JSON in configuration file: {appSettingsPath}", ex);
        }

        // Convert to mutable dictionary structure
        Dictionary<string, object> configRoot = ConvertJsonElementToDictionary(document.RootElement);
        document.Dispose();

        // Update the value using the key path
        UpdateNestedValue(configRoot, key, value);

        // Convert back to JSON and write to file
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string updatedJson = JsonSerializer.Serialize(configRoot, options);

        await fileSystemService.FileWriteAllTextAsync(appSettingsPath, updatedJson, cancellationToken);

        logger.LogInformation("Successfully updated runtime configuration");
    }

    private static Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonElementToObject(property.Value);
        }

        return dictionary;
    }

    private static object ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonElementToDictionary(element),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElementToObject).ToArray(),
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out int intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.ToString()
        };
    }

    private static void UpdateNestedValue(Dictionary<string, object> root, string keyPath, object value)
    {
        var keys = keyPath.Split(':');
        var current = root;

        // Navigate through all keys except the last one
        for (int i = 0; i < keys.Length - 1; i++)
        {
            var key = keys[i];

            if (!current.ContainsKey(key))
            {
                // Create new nested object if it doesn't exist
                current[key] = new Dictionary<string, object>();
            }
            else if (current[key] is not Dictionary<string, object>)
            {
                // Replace non-object value with new nested object
                current[key] = new Dictionary<string, object>();
            }

            current = (Dictionary<string, object>)current[key];
        }

        // Set the final value
        var finalKey = keys[^1];
        current[finalKey] = value;
    }
}
