namespace HomeBook.Backend.Module.Kitchen.Models;

public record RecipeRequestDto(
    Guid? Id,
    Guid UserId,
    string Name,
    string? Description,
    int? Servings,
    RecipeMediaItemRequestDto[] MediaItems,
    RecipeIngredientRequestDto[] Ingredients,
    RecipeStepRequestDto[] Steps,
    int? DurationWorkingMinutes,
    int? DurationCookingMinutes,
    int? DurationRestingMinutes,
    int? CaloriesKcal,
    string? Comments,
    string? Source)
{
    public Guid[] MediaIds => MediaItems
        .OrderBy(x => x.Index)
        .Select(x => x.MediaItemId)
        .ToArray();
}
