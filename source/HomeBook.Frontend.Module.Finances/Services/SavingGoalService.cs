using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Module.Finances.Contracts;
using HomeBook.Frontend.Module.Finances.Mappings;
using HomeBook.Frontend.Module.Finances.Models;

namespace HomeBook.Frontend.Module.Finances.Services;

/// <inheritdoc />
public class SavingGoalService(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : ISavingGoalService
{
    /// <inheritdoc />
    public async Task<IEnumerable<SavingGoalDto>> GetAllSavingGoalsAsync(CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        SavingGoalListResponse? response = await backendClient.Modules.Homebook.Finances.SavingGoals.GetAsync(x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");
            },
            cancellationToken);

        if (response is null)
            return [];

        List<SavingGoalDto> result = (response.SavingGoals ?? [])
            .Select(x => x.ToDto())
            .OrderByDescending(x => x.Percentage)
            .ToList();

        return result;
    }

    /// <inheritdoc />
    public Task<SavingGoalDto?> GetSavingGoalByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Guid> CreateSavingGoalAsync(SavingGoalDto savingGoal, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task UpdateSavingGoalAsync(SavingGoalDto savingGoal, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
