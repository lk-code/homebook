using System.Security.Claims;
using HomeBook.Backend.Core.Modules.Utilities;
using HomeBook.Backend.Module.Finances.Contracts;
using HomeBook.Backend.Module.Finances.Enums;
using HomeBook.Backend.Module.Finances.Mappings;
using HomeBook.Backend.Module.Finances.Models;
using HomeBook.Backend.Module.Finances.Requests;
using HomeBook.Backend.Module.Finances.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Module.Finances.Handler;

public class SavingGoalHandler
{
    /// <summary>
    /// gets the user finance saving goals
    /// </summary>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleGetSavingGoals(ClaimsPrincipal user,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            SavingGoalDto[] savingGoalDtos = await savingGoalsProvider.GetAllSavingGoalsAsync(userId,
                cancellationToken);
            SavingGoalResponse[] savingGoals = savingGoalDtos.Select(sg => sg.ToResponse()).ToArray();

            return TypedResults.Ok(new SavingGoalListResponse(savingGoals));
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while retrieving saving goals");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleCreateSavingGoal(ClaimsPrincipal user,
        [FromBody] CreateSavingGoalRequest request,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            Guid createdId = await savingGoalsProvider.CreateAsync(userId,
                request.Name,
                request.Color,
                request.Icon,
                request.TargetAmount,
                request.CurrentAmount,
                request.MonthlyPayment,
                (InterestRateOptions?)request.InterestRateOption,
                request.InterestRate,
                request.TargetDate,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while creating saving goal");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="savingGoalId"></param>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleUpdateSavingGoalName(Guid savingGoalId,
        ClaimsPrincipal user,
        [FromBody] UpdateSavingGoalNameRequest request,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await savingGoalsProvider.UpdateSavingGoalNameAsync(userId,
                savingGoalId,
                request.Name,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating saving goal name");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="savingGoalId"></param>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleUpdateSavingGoalAppearance(Guid savingGoalId,
        ClaimsPrincipal user,
        [FromBody] UpdateSavingGoalAppearanceRequest request,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await savingGoalsProvider.UpdateSavingGoalAppearanceAsync(userId,
                savingGoalId,
                request.Color,
                request.Icon,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating saving goal appearance");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="savingGoalId"></param>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleUpdateSavingGoalAmounts(Guid savingGoalId,
        ClaimsPrincipal user,
        [FromBody] UpdateSavingGoalAmountsRequest request,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await savingGoalsProvider.UpdateSavingGoalAmountsAsync(userId,
                savingGoalId,
                request.TargetAmount,
                request.CurrentAmount,
                request.MonthlyPayment,
                (InterestRateOptions?)request.InterestRateOption,
                request.InterestRate,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating saving goal amounts");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="savingGoalId"></param>
    /// <param name="user"></param>
    /// <param name="request"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleUpdateSavingGoalInfo(Guid savingGoalId,
        ClaimsPrincipal user,
        [FromBody] UpdateSavingGoalInfoRequest request,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await savingGoalsProvider.UpdateSavingGoalInfoAsync(userId,
                savingGoalId,
                request.TargetDate,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while updating saving goal information");
            return TypedResults.InternalServerError(err.Message);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    /// <param name="savingGoalsProvider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<IResult> HandleDeleteSavingGoal(Guid id,
        ClaimsPrincipal user,
        [FromServices] ILogger<SavingGoalHandler> logger,
        [FromServices] ISavingGoalsProvider savingGoalsProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            Guid userId = user.GetUserId();

            await savingGoalsProvider.DeleteAsync(userId,
                id,
                cancellationToken);

            return TypedResults.Ok();
        }
        catch (Exception err)
        {
            logger.LogError(err, "Error while deleting saving goal");
            return TypedResults.InternalServerError(err.Message);
        }
    }
}
