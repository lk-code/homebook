using FluentValidation;
using HomeBook.Backend.Abstractions;
using HomeBook.Backend.Abstractions.Setup;
using Homebook.Backend.Core.Setup.Models;
using Microsoft.Extensions.Logging;

namespace Homebook.Backend.Core.Setup.Provider;

public class SetupConfigurationProvider(
    ILogger<SetupConfigurationProvider> logger,
    IValidator<EnvironmentConfiguration> environmentValidator)
    : ISetupConfigurationProvider
{
    private Dictionary<EnvironmentVariables, string?>? _valuesByEnum;

    private void LoadEnvironmentConfiguration(IValidator<EnvironmentConfiguration> environmentValidator)
    {
        if (_valuesByEnum is not null)
            return;

        _valuesByEnum = new Dictionary<EnvironmentVariables, string?>();

        _valuesByEnum.Clear();
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

        foreach (EnvironmentVariables varName in Enum.GetValues<EnvironmentVariables>())
        {
            string? value = Environment.GetEnvironmentVariable(varName.ToString());
            _valuesByEnum[varName] = value;
        }

        // displaying the loaded environment variables for debugging purposes
        logger.LogInformation("Loaded environment variables:");
        foreach (var kvp in _valuesByEnum)
        {
            logger.LogInformation("{VariableName}: {Value}", kvp.Key, kvp.Value ?? "null");
        }
    }

    public string? GetValue(EnvironmentVariables name)
    {
        LoadEnvironmentConfiguration(environmentValidator);

        _valuesByEnum!.TryGetValue(name, out var value);

        return value;
    }
}
