using System.Text.RegularExpressions;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Setup;

namespace Homebook.Backend.Core.Setup.Provider;

public class SetupConfigurationProvider : ISetupConfigurationProvider
{
    private static readonly Regex HostRegex = new Regex("^[A-Za-z0-9.-]{1,253}$", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new Regex("^[A-Za-z0-9_.-]{1,64}$", RegexOptions.Compiled);
    private const int MaxPasswordLength = 256;

    private readonly Dictionary<EnvironmentVariables, string?> _valuesByEnum;

    public SetupConfigurationProvider()
    {
        _valuesByEnum = new Dictionary<EnvironmentVariables, string?>();
        foreach (var varName in Enum.GetValues<EnvironmentVariables>())
        {
            var value = Environment.GetEnvironmentVariable(varName.ToString());
            _valuesByEnum[varName] = value;
        }
    }

    public bool IsSet(EnvironmentVariables name)
    {
        return _valuesByEnum.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value);
    }

    public string? GetValue(EnvironmentVariables name)
    {
        _valuesByEnum.TryGetValue(name, out var value);
        return value;
    }

    public bool TryGetValueValidated(EnvironmentVariables name, out string? value, out string? error)
    {
        _valuesByEnum.TryGetValue(name, out value);
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            value = null;
            error = null;
            return false;
        }

        switch (name)
        {
            case EnvironmentVariables.DATABASE_HOST:
                if (HostRegex.IsMatch(value))
                {
                    return true;
                }
                error = "Invalid value for DATABASE_HOST";
                return false;
            case EnvironmentVariables.DATABASE_PORT:
                if (int.TryParse(value, out int port) && port >= 1 && port <= 65535)
                {
                    return true;
                }
                error = "Invalid value for DATABASE_PORT";
                return false;
            case EnvironmentVariables.DATABASE_NAME:
            case EnvironmentVariables.DATABASE_USER:
                if (NameRegex.IsMatch(value))
                {
                    return true;
                }
                error = $"Invalid value for {name}";
                return false;
            case EnvironmentVariables.DATABASE_PASSWORD:
                if (value.Length <= MaxPasswordLength && !value.Any(char.IsControl))
                {
                    return true;
                }
                error = "Invalid value for DATABASE_PASSWORD";
                return false;
            default:
                // No validation for other variables, consider valid
                return true;
        }
    }

    public string? GetValidatedOrNull(EnvironmentVariables name)
    {
        if (TryGetValueValidated(name, out var value, out var error))
        {
            return value;
        }
        return null;
    }
}
