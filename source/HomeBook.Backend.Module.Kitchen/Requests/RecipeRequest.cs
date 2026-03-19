namespace HomeBook.Backend.Module.Kitchen.Requests;

public record RecipeRequest(
    string Name,
    string? Description,
    int? Servings,
    CreateRecipeIngredientRequest[]? Ingredients,
    CreateRecipeStepRequest[]? Steps,
    int? DurationWorkingMinutes,
    int? DurationCookingMinutes,
    int? DurationRestingMinutes,
    int? CaloriesKcal,
    string? Comments,
    string? Source,
    CreateRecipeMediaItemRequest[]? MediaItems,
    Guid[]? MediaIds);
