using System.Text.RegularExpressions;
using FluentValidation;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.UserPreferences;
using HomeBook.Backend.Core.DataProvider.Exceptions;
using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Core.DataProvider;

/// <inheritdoc />
public partial class UserPreferenceProvider(
    ILogger<UserPreferenceProvider> logger,
    IUserPreferenceRepository userPreferenceRepository,
    IValidator<UserPreference> userPreferenceValidator) : IUserPreferenceProvider
{
    private static readonly string PREFERENCE_KEY_LOCALE = "LOCALE";
    private static readonly string PREFERENCE_KEY_WALLPAPER = "WALLPAPER";

    [GeneratedRegex(@"^\{[a-z0-9]{5}\}-\{.+\}$")]
    private static partial Regex WallpaperPreferenceValidationRegex();

    /// <inheritdoc />
    public async Task<string?> GetUserPreferredLocaleAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving locale preference");

        UserPreference? userPreference = await userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId,
                PREFERENCE_KEY_LOCALE,
                cancellationToken);
        return userPreference?.Value ?? null;
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

        await userPreferenceRepository.SetPreferenceAsync(userPreference,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WallpaperConfiguration?> GetUserWallpaperAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving wallpaper preference");

        UserPreference? userPreference = await userPreferenceRepository
            .GetPreferenceForUserByKeyAsync(userId,
                PREFERENCE_KEY_WALLPAPER,
                cancellationToken);

        if (userPreference is null)
            return null;

        string[] parts = userPreference.Value.Split("}-{", 2);
        string type = parts[0].TrimStart('{');
        string key = parts[1].TrimEnd('}');
        return new WallpaperConfiguration(type, key);
    }

    /// <inheritdoc />
    public async Task SetUserWallpaperAsync(Guid userId,
        string wallpaperConfiguration,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Setting user preference for wallpaper. UserId: {UserId}, WallpaperConfiguration: {WallpaperConfiguration}",
            userId,
            wallpaperConfiguration);

        // verify that the string is in this format: {type}-{key}
        bool isValid = WallpaperPreferenceValidationRegex().IsMatch(wallpaperConfiguration);
        if (!isValid)
            throw new InvalidPreferenceException(
                "Invalid wallpaper configuration format. Expected format: {type}-{key} where type is a 5 character lowercase alphanumeric string.");

        string[] parts = wallpaperConfiguration.Split("}-{", 2);
        string type = parts[0].TrimStart('{');
        string key = parts[1].TrimEnd('}');

        string[] validTypes =
        [
            // dynamic wallpaper        => {dynwp}-{WallpaperName:string}
            "dynwp",
            // static wallpaper         => {stawp}-{FilePath:string}
            "stawp",
            // user uploaded wallpaper  => {usrwp}-{MediaId:Guid}
            "usrwp"
        ];
        if (!validTypes.Contains(type)
            || string.IsNullOrEmpty(key))
            throw new InvalidPreferenceException(
                "Invalid wallpaper configuration. Type must be one of: dynwp, stawp, usrwp and key must not be empty.");

        string wallpaperConfigValue = $"{{{type}}}-{{{key}}}";
        UserPreference userPreference = new()
        {
            UserId = userId,
            Key = PREFERENCE_KEY_WALLPAPER,
            Value = wallpaperConfigValue
        };
        await userPreferenceValidator.ValidateAndThrowAsync(userPreference,
            cancellationToken: cancellationToken);

        await userPreferenceRepository.SetPreferenceAsync(userPreference,
            cancellationToken);
    }
}
