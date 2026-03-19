namespace HomeBook.Backend.Module.Kitchen.Models;

public record RecipeResultDto(
    Guid Id,
    Guid? UserId,
    string Name,
    string NormalizedName,
    string? Description,
    int? Servings,
    int? DurationWorkingMinutes,
    int? DurationCookingMinutes,
    int? DurationRestingMinutes,
    int? CaloriesKcal,
    string? Comments,
    string? Source,
    RecipeIngredientDto[] Ingredients,
    RecipeStepDto[] Steps,
    RecipeMediaItemDto[] MediaItems)
{
    public Guid[] MediaIds => MediaItems
        .OrderBy(x => x.Index)
        .Select(x => x.MediaItemId)
        .ToArray();
}
