namespace HomeBook.Backend.Module.Kitchen.Models;

public record RecipeRequestDto(
    Guid? Id,
    Guid UserId,
    string Name,
    string? Description,
    int? Servings,
    Guid[] MediaIds,
    RecipeIngredientRequestDto[] Ingredients,
    RecipeStepRequestDto[] Steps,
    int? DurationWorkingMinutes,
    int? DurationCookingMinutes,
    int? DurationRestingMinutes,
    int? CaloriesKcal,
    string? Comments,
    string? Source);
