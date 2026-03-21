using FluentValidation;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Core.DataProvider;

/// <inheritdoc />
public class UserPreferenceProvider(
    ILogger<UserPreferenceProvider> logger,
    IUserPreferenceRepository userPreferenceRepository,
    IValidator<UserPreference> userPreferenceValidator) : IUserPreferenceProvider
{
    private static readonly string PREFERENCE_KEY_LOCALE = "LOCALE";

    /// <inheritdoc />
    public async Task<string?> GetUserPreferredLocaleAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving locale preference");

        UserPreference? localeUserPreference = await userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId,
                PREFERENCE_KEY_LOCALE,
                cancellationToken);
        return localeUserPreference?.Value ?? null;
    }

    /// <inheritdoc />
    public async Task SetUserPreferredLocaleAsync(Guid userId,
        string locale,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Setting user preference for locale. UserId: {UserId}, Locale: {Locale}",
            userId,
            locale);

        UserPreference userPreference = new()
        {
            UserId = userId,
            Key = PREFERENCE_KEY_LOCALE,
            Value = locale
        };
        await userPreferenceValidator.ValidateAndThrowAsync(userPreference,
            cancellationToken: cancellationToken);

        await userPreferenceRepository.SetPreferenceForUserByKeyAsync(userPreference,
            cancellationToken);
    }
}
