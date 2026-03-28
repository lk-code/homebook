namespace HomeBook.Frontend.Module.Kitchen.Models;

public record RecipeDetailDto(
    Guid Id,
    string Username,
    string Name,
    string NormalizedName,
    string Description,
    int? Servings,
    RecipeMediaItemDto[] ImageMediaItems,
    Guid[] ImageMediaIds,
    Guid? HeroMediaId,
    RecipeIngredientDto[] Ingredients,
    RecipeStepDto[] Steps,
    int? DurationWorkingMinutes,
    int? DurationCookingMinutes,
    int? DurationRestingMinutes,
    int? CaloriesKcal,
    string Comments,
    string Source);
