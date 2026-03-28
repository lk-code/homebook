namespace HomeBook.Frontend.Module.Kitchen.Models;

public record RecipeDto(
    Guid Id,
    string Username,
    string Name,
    string Description,
    int? Servings,
    int? CaloriesKcal,
    int? DurationInMinutes,
    string Ingredients,
    Guid? HeroMediaId);
