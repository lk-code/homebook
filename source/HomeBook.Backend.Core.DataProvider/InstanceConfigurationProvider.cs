using FluentValidation;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Core.DataProvider;

/// <inheritdoc />
public class InstanceConfigurationProvider(
    IConfigurationRepository configurationRepository,
    IValidator<Configuration> configurationValidator,
    ILogger<InstanceConfigurationProvider> logger) : IInstanceConfigurationProvider
{
    private static readonly string HOMEBOOK_CONFIGURATION_NAME = "HOMEBOOK_CONFIGURATION_NAME";
    private static readonly string HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE = "HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE";

    /// <inheritdoc />
    public async Task SetHomeBookInstanceNameAsync(string instanceName,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating HomeBook instance name");

        Configuration config = new()
        {
            Key = HOMEBOOK_CONFIGURATION_NAME,
            Value = instanceName
        };
        await configurationValidator.ValidateAndThrowAsync<Configuration>(config,
            cancellationToken: cancellationToken);

        await configurationRepository.WriteConfigurationAsync(config, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GetHomeBookInstanceNameAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving HomeBook instance name");

        Configuration? instanceName = await configurationRepository
            .GetConfigurationByKeyAsync(HOMEBOOK_CONFIGURATION_NAME,
                cancellationToken);
        return instanceName?.Value ?? string.Empty;
    }

    public async Task SetHomeBookInstanceDefaultLocaleAsync(string defaultLocale,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating HomeBook default locale");

        Configuration config = new()
        {
            Key = HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE,
            Value = defaultLocale
        };
        await configurationValidator.ValidateAndThrowAsync<Configuration>(config,
            cancellationToken: cancellationToken);

        await configurationRepository.WriteConfigurationAsync(config, cancellationToken);
    }

    public async Task<string?> GetHomeBookInstanceDefaultLocaleAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving HomeBook default locale");

        Configuration? defaultLanguage = await configurationRepository
            .GetConfigurationByKeyAsync(HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE,
                cancellationToken);
        return defaultLanguage?.Value ?? null;
    }
}
