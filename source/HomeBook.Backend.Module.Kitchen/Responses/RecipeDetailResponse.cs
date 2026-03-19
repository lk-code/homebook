using System.Diagnostics;

namespace HomeBook.Backend.Module.Kitchen.Responses;

[DebuggerDisplay("{Name}")]
public record RecipeDetailResponse(
    Guid Id,
    string? Username,
    string Name,
    string NormalizedName,
    string? Description,
    int? Servings,
    Guid[] MediaIds,
    RecipeMediaItemResponse[] MediaItems,
    RecipeIngredientResponse[] Ingredients,
    RecipeStepResponse[] Steps,
    int? DurationWorkingMinutes,
    int? DurationCookingMinutes,
    int? DurationRestingMinutes,
    int? CaloriesKcal,
    string? Comments,
    string? Source);
