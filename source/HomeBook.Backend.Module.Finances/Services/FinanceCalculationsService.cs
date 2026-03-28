using HomeBook.Backend.Abstractions.Contracts;
using HomeBook.Backend.Module.Finances.Contracts;
using HomeBook.Backend.Module.Finances.Models;
using Microsoft.Extensions.Logging;

namespace HomeBook.Backend.Module.Finances.Services;

/// <inheritdoc/>
public class FinanceCalculationsService(
    IDateTimeProvider dateTimeProvider,
    ILogger<FinanceCalculationsService> logger)
    : IFinanceCalculationsService
{
    public SavingCalculationResult CalculateSavings(decimal targetAmount,
        DateTime targetDate,
        bool targetSimpleRate = true)
    {
        logger.LogInformation("Calculating savings projection");

        var now = dateTimeProvider.UtcNow;
        var totalMonths = ((targetDate.Year - now.Year) * 12) + targetDate.Month - now.Month;
        if (totalMonths <= 0)
            return new SavingCalculationResult(0,
                0,
                [],
                []);

        var monthlyPayment = targetAmount / totalMonths;

        // Wenn einfache Rate aktiviert ist: auf den nächsten 5er-Schritt aufrunden
        if (targetSimpleRate)
        {
            monthlyPayment = Math.Ceiling(monthlyPayment / 5) * 5;
        }

        var amounts = new decimal[totalMonths];
        for (var monthIndex = 0; monthIndex < totalMonths; monthIndex++)
        {
            amounts[monthIndex] = monthlyPayment * (monthIndex + 1);
        }

        return new SavingCalculationResult((short)totalMonths,
            monthlyPayment,
            amounts,
            []);
    }

    /// <inheritdoc/>
    public SavingCalculationResult CalculateMonthlySavings(decimal targetAmount,
        DateTime targetDate,
        decimal interestRate,
        bool targetSimpleRate = true)
    {
        logger.LogInformation("Calculating monthly savings projection");

        var now = dateTimeProvider.UtcNow;
        var totalMonths = ((targetDate.Year - now.Year) * 12) + targetDate.Month - now.Month;
        if (totalMonths <= 0)
            return new SavingCalculationResult(0, 0, [], []);

        var monthlyRate = (interestRate / 100m) / 12m;

        decimal monthlyPayment;

        if (monthlyRate == 0)
        {
            // Kein Zins → einfache lineare Aufteilung
            monthlyPayment = targetAmount / totalMonths;
        }
        else
        {
            // Zinseszinsformel
            var divisor = ((decimal)Math.Pow((double)(1 + monthlyRate), totalMonths) - 1) / monthlyRate;
            monthlyPayment = targetAmount / divisor;
        }

        if (targetSimpleRate)
            monthlyPayment = Math.Ceiling(monthlyPayment / 5) * 5;

        var amounts = new decimal[totalMonths];
        var interests = new decimal[totalMonths];
        decimal balance = 0;

        for (var i = 0; i < totalMonths; i++)
        {
            balance += monthlyPayment;
            var interest = balance * monthlyRate;
            balance += interest;

            interests[i] = Math.Round(interest, 2);
            amounts[i] = Math.Round(balance, 2);
        }

        return new SavingCalculationResult((short)totalMonths,
            monthlyPayment,
            amounts,
            interests);
    }

    /// <inheritdoc/>
    public SavingCalculationResult CalculateYearlySavings(decimal targetAmount,
        DateTime targetDate,
        decimal interestRate,
        bool targetSimpleRate = true)
    {
        logger.LogInformation("Calculating yearly savings projection");

        var now = dateTimeProvider.UtcNow;
        var totalMonths = ((targetDate.Year - now.Year) * 12) + targetDate.Month - now.Month;
        if (totalMonths <= 0)
            return new SavingCalculationResult(0, 0, [], []);

        var yearlyRate = interestRate / 100m;

        // Monatliche Rate ohne Zinseszins-Berechnung (lineare Aufteilung)
        var monthlyPayment = targetAmount / totalMonths;

        if (targetSimpleRate)
            monthlyPayment = Math.Ceiling(monthlyPayment / 5) * 5;

        var amounts = new decimal[totalMonths];
        var interests = new decimal[totalMonths];
        decimal balance = 0;

        for (var i = 0; i < totalMonths; i++)
        {
            // Monatliche Einzahlung
            balance += monthlyPayment;

            // Nur am Ende eines Jahres (nach 12, 24, 36, … Monaten) Zinsen berechnen
            if ((i + 1) % 12 == 0)
            {
                var interest = balance * yearlyRate;
                balance += interest;
                interests[i] = Math.Round(interest, 2);
            }
            else
            {
                interests[i] = 0;
            }

            // Gesamtbetrag speichern (immer nach Monatsabschluss)
            amounts[i] = Math.Round(balance, 2);
        }

        return new SavingCalculationResult((short)totalMonths,
            monthlyPayment,
            amounts,
            interests);
    }
}
