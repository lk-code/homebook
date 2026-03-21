using HomeBook.Backend.Data.Contracts;
using HomeBook.Backend.Data.Entities;
using HomeBook.Backend.Module.Finances.Contracts;
using HomeBook.Backend.Module.Finances.Enums;
using HomeBook.Backend.Module.Finances.Mappings;
using HomeBook.Backend.Module.Finances.Models;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Module.Finances.Provider;

public class SavingGoalsProvider(
    ILogger<SavingGoalsProvider> logger,
    ISavingGoalsRepository savingGoalsRepository) : ISavingGoalsProvider
{
    /// <inheritdoc />
    public async Task<SavingGoalDto[]> GetAllSavingGoalsAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving saving goals");

        IEnumerable<SavingGoal> savingGoalEntities = await savingGoalsRepository.GetAllSavingGoalsAsync(userId,
            cancellationToken);
        SavingGoalDto[] savingGoals = savingGoalEntities
            .Select(sg => sg.ToDto())
            .ToArray();

        return savingGoals;
    }

    /// <inheritdoc />
    public async Task<SavingGoalDto?> GetSavingGoalByIdAsync(Guid userId,
        Guid savingGoalId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving saving goal");
        return (await savingGoalsRepository.GetByIdAsync(userId,
            savingGoalId,
            cancellationToken))?.ToDto();
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(Guid userId,
        string name,
        string color,
        string icon,
        decimal targetAmount,
        decimal currentAmount,
        decimal monthlyPayment,
        InterestRateOptions? interestRateOption,
        decimal? interestRate,
        DateTime? targetDate,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating saving goal");

        SavingGoal entity = new()
        {
            UserId = userId,
            Name = name,
            Color = color,
            Icon = icon,
            TargetAmount = targetAmount,
            CurrentAmount = currentAmount,
            MonthlyPayment = monthlyPayment,
            InterestRateOption = (SavingGoal.InterestRateOptions)interestRateOption!,
            InterestRate = interestRate,
            TargetDate = targetDate?.ToUniversalTime().Date ?? null
        };

        // TODO: validator

        Guid entityId = await savingGoalsRepository
            .CreateOrUpdateAsync(userId,
                entity,
                cancellationToken);
        return entityId;
    }

    /// <inheritdoc />
    public async Task UpdateSavingGoalNameAsync(Guid userId,
        Guid savingGoalId,
        string name,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating saving goal name");

        SavingGoal entity = await savingGoalsRepository.GetByIdAsync(userId,
                                savingGoalId,
                                cancellationToken)
                            ?? throw new KeyNotFoundException(
                                $"Saving goal with ID {savingGoalId} not found for user {userId}.");

        entity.Name = name;

        await savingGoalsRepository.CreateOrUpdateAsync(userId,
            entity,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateSavingGoalAppearanceAsync(Guid userId,
        Guid savingGoalId,
        string color,
        string icon,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating saving goal appearance");

        SavingGoal entity = await savingGoalsRepository.GetByIdAsync(userId,
                                savingGoalId,
                                cancellationToken)
                            ?? throw new KeyNotFoundException(
                                $"Saving goal with ID {savingGoalId} not found for user {userId}.");

        entity.Color = color;
        entity.Icon = icon;

        await savingGoalsRepository.CreateOrUpdateAsync(userId,
            entity,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateSavingGoalAmountsAsync(Guid userId,
        Guid savingGoalId,
        decimal? targetAmount,
        decimal? currentAmount,
        decimal? monthlyPayment,
        InterestRateOptions? interestRateOption,
        decimal? interestRate,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating saving goal amounts");

        SavingGoal entity = await savingGoalsRepository.GetByIdAsync(userId,
                                savingGoalId,
                                cancellationToken)
                            ?? throw new KeyNotFoundException(
                                $"Saving goal with ID {savingGoalId} not found for user {userId}.");

        if (targetAmount.HasValue)
            entity.TargetAmount = targetAmount.Value;
        if (currentAmount.HasValue)
            entity.CurrentAmount = currentAmount.Value;
        if (monthlyPayment.HasValue)
            entity.MonthlyPayment = monthlyPayment.Value;
        if (interestRateOption.HasValue)
            entity.InterestRateOption = (SavingGoal.InterestRateOptions)interestRateOption.Value;
        if (interestRate.HasValue)
            entity.InterestRate = interestRate.Value;

        await savingGoalsRepository.CreateOrUpdateAsync(userId,
            entity,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateSavingGoalInfoAsync(Guid userId,
        Guid savingGoalId,
        DateTime? targetDate,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating saving goal info");

        SavingGoal entity = await savingGoalsRepository.GetByIdAsync(userId,
                                savingGoalId,
                                cancellationToken)
                            ?? throw new KeyNotFoundException(
                                $"Saving goal with ID {savingGoalId} not found for user {userId}.");

        if (targetDate.HasValue)
            entity.TargetDate = targetDate.Value;

        await savingGoalsRepository.CreateOrUpdateAsync(userId,
            entity,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid userId,
        Guid savingGoalId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting saving goal");
        await savingGoalsRepository.DeleteAsync(userId,
            savingGoalId,
            cancellationToken);
    }
}
