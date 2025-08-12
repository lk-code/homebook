using System.Text.RegularExpressions;
using FluentValidation;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Setup;
using Homebook.Backend.Core.Setup.Models;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup.Provider;

public class SetupConfigurationProvider : ISetupConfigurationProvider
{
    private readonly ILogger<SetupConfigurationProvider> _logger;
    private readonly Dictionary<EnvironmentVariables, string?> _valuesByEnum;

    private static readonly Regex HostRegex = new Regex("^[A-Za-z0-9.-]{1,253}$", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new Regex("^[A-Za-z0-9_.-]{1,64}$", RegexOptions.Compiled);
    private const int MaxPasswordLength = 256;

    public SetupConfigurationProvider(ILogger<SetupConfigurationProvider> logger,
        IValidator<EnvironmentConfiguration> environmentValidator)
    {
        _logger = logger;

        EnvironmentConfiguration environmentConfiguration = new(
            Environment.GetEnvironmentVariable(EnvironmentVariables.DATABASE_HOST.ToString()),
            Environment.GetEnvironmentVariable(EnvironmentVariables.DATABASE_PORT.ToString()),
            Environment.GetEnvironmentVariable(EnvironmentVariables.DATABASE_NAME.ToString()),
            Environment.GetEnvironmentVariable(EnvironmentVariables.DATABASE_USER.ToString()),
            Environment.GetEnvironmentVariable(EnvironmentVariables.DATABASE_PASSWORD.ToString()),
            Environment.GetEnvironmentVariable(EnvironmentVariables.HOMEBOOK_USER_NAME.ToString()),
            Environment.GetEnvironmentVariable(EnvironmentVariables.HOMEBOOK_USER_PASSWORD.ToString())
        );
        environmentValidator.ValidateAndThrow(environmentConfiguration);

        _valuesByEnum = new Dictionary<EnvironmentVariables, string?>();
        foreach (EnvironmentVariables varName in Enum.GetValues<EnvironmentVariables>())
        {
            string? value = Environment.GetEnvironmentVariable(varName.ToString());
            _valuesByEnum[varName] = value;
        }

        // displaying the loaded environment variables for debugging purposes
        _logger.LogInformation("Loaded environment variables:");
        foreach (var kvp in _valuesByEnum)
        {
            _logger.LogInformation("{VariableName}: {Value}", kvp.Key, kvp.Value ?? "null");
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
            case EnvironmentVariables.DATABASE_PORT:
            case EnvironmentVariables.DATABASE_NAME:
            case EnvironmentVariables.DATABASE_USER:
            case EnvironmentVariables.DATABASE_PASSWORD:
            case EnvironmentVariables.HOMEBOOK_USER_NAME:
            case EnvironmentVariables.HOMEBOOK_USER_PASSWORD:
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
