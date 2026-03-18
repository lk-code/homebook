using HomeBook.Client;
using HomeBook.Client.Models;
using HomeBook.Frontend.Abstractions.Contracts;
using HomeBook.Frontend.Module.Kitchen.Contracts;
using HomeBook.Frontend.Module.Kitchen.Mappings;
using HomeBook.Frontend.Module.Kitchen.Models;

namespace HomeBook.Frontend.Module.Kitchen.Services;

/// <inheritdoc/>
public class RecipeService(
    IAuthenticationService authenticationService,
    BackendClient backendClient) : IRecipeService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<RecipeDto>> GetRecipesAsync(string? filter,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        RecipesListResponse? response = await backendClient.Modules.Homebook.Kitchen.Recipes.GetAsync(x =>
            {
                x.Headers.Add("Authorization", $"Bearer {token}");

                if (!string.IsNullOrWhiteSpace(filter))
                    x.QueryParameters.SearchFilter = filter;
            },
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (response is null)
            return [];

        List<RecipeDto> result = (response.Recipes ?? [])
            .Select(x => x.ToDto())
            .ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<RecipeDetailDto?> GetRecipeByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        RecipeDetailResponse? response = await backendClient.Modules.Homebook.Kitchen.Recipes[id]
            .GetAsync(x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);

        return response?.ToDto();
    }

    /// <inheritdoc/>
    public async Task CreateOrUpdateRecipeAsync(Guid? id,
        string name,
        string? description = null,
        int? servings = null,
        RecipeStepDto[]? steps = null,
        RecipeIngredientDto[]? ingredients = null,
        int? durationWorkingMinutes = null,
        int? durationCookingMinutes = null,
        int? durationRestingMinutes = null,
        int? caloriesKcal = null,
        string? comments = null,
        string? source = null,
        List<Guid>? mediaIds = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        RecipeRequest request = new()
        {
            Name = name,
            Description = description,
            DurationWorkingMinutes = durationWorkingMinutes,
            DurationCookingMinutes = durationCookingMinutes,
            DurationRestingMinutes = durationRestingMinutes,
            CaloriesKcal = caloriesKcal,
            Servings = servings,
            Comments = comments,
            Source = source,
            MediaIds = mediaIds?.Select(x => (Guid?)x).ToList(),
            Ingredients = (ingredients ?? []).Select(x => x.ToRequest()).ToList(),
            Steps = (steps ?? []).Select(x => x.ToRequest()).ToList()
        };

        if (id.HasValue)
        {
            // Update existing recipe
            await backendClient.Modules.Homebook.Kitchen.Recipes[id.Value]
                .PutAsync(request,
                    x =>
                    {
                        x.Headers.Add("Authorization", $"Bearer {token}");
                    },
                    cancellationToken);
        }
        else
        {
            // Create new recipe
            await backendClient.Modules.Homebook.Kitchen.Recipes.PostAsync(request,
                x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task CreateRecipeAsync(string name,
        CancellationToken cancellationToken = default) =>
        await CreateOrUpdateRecipeAsync(null,
            name,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            cancellationToken);

    /// <inheritdoc/>
    public async Task DeleteRecipeAsync(Guid recipeId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        await backendClient.Modules.Homebook.Kitchen.Recipes[recipeId]
            .DeleteAsync(x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateRecipeNameAsync(Guid recipeId,
        string name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? token = await authenticationService.GetTokenAsync(cancellationToken);
        await backendClient.Modules.Homebook.Kitchen.Recipes[recipeId]
            .PatchAsync(
                new RecipeRenameRequest
                {
                    Name = name
                },
                x =>
                {
                    x.Headers.Add("Authorization", $"Bearer {token}");
                },
                cancellationToken);
    }
}
