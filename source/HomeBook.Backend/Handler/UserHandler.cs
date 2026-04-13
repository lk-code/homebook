using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Abstractions.Models.UserPreferences;
using HomeBook.Backend.Core.Modules.Utilities;
using HomeBook.Backend.Requests;
using HomeBook.Backend.Responses;

namespace HomeBook.Backend.Handler;

public class UserHandler
{
    /// <summary>
    /// gets the user preference for locale
    /// </summary>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    /// <param name="userPreferenceProvider"></param>
    /// <param name="instanceConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetUserPreferenceForLocale(ClaimsPrincipal user,
        [FromServices] ILogger<UserHandler> logger,
        [FromServices] IUserPreferenceProvider userPreferenceProvider,
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            string? locale = await userPreferenceProvider.GetUserPreferredLocaleAsync(userId,
                cancellationToken);

            if (string.IsNullOrEmpty(locale))
                locale = await instanceConfigurationProvider.GetHomeBookInstanceDefaultLocaleAsync(cancellationToken);

            return TypedResults.Ok(new GetUserPreferenceLocaleResponse(locale!));
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving user locale preference");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// updates the user preference for locale
    /// </summary>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    /// <param name="userPreferenceProvider"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleUpdateUserPreferenceForLocale(ClaimsPrincipal user,
        [FromServices] ILogger<UserHandler> logger,
        [FromServices] IUserPreferenceProvider userPreferenceProvider,
        [FromBody] UpdateUserPreferenceLocaleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await userPreferenceProvider.SetUserPreferredLocaleAsync(userId,
                request.Locale,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating user locale preference");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// gets the user preference for locale
    /// </summary>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    /// <param name="userPreferenceProvider"></param>
    /// <param name="instanceConfigurationProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetUserPreferenceForWallpaper(ClaimsPrincipal user,
        [FromServices] ILogger<UserHandler> logger,
        [FromServices] IUserPreferenceProvider userPreferenceProvider,
        [FromServices] IInstanceConfigurationProvider instanceConfigurationProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            WallpaperConfiguration? wallpaperConfig = await userPreferenceProvider.GetUserWallpaperAsync(userId,
                cancellationToken);

            if (wallpaperConfig is null)
                return TypedResults.NotFound();

            // TODO: if user uploaded image, then return mediaId, if static wallpaper, return the link to the image, if dynamic wallpaper, return the name of the wallpaper

            return TypedResults.Ok(new GetUserPreferenceWallpaperResponse(
                wallpaperConfig.Config,
                wallpaperConfig.Type,
                wallpaperConfig.WallpaperKey));
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving user locale preference");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    /// Updates the user preference for wallpaper.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    /// <param name="userPreferenceProvider"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleUpdateUserPreferenceForWallpaper(ClaimsPrincipal user,
        [FromServices] ILogger<UserHandler> logger,
        [FromServices] IUserPreferenceProvider userPreferenceProvider,
        [FromBody] UpdateUserPreferenceWallpaperRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await userPreferenceProvider.SetUserWallpaperAsync(userId,
                request.WallpaperConfiguration,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating user wallpaper preference");
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
