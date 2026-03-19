using System.Diagnostics;

namespace HomeBook.Backend.Module.Kitchen.Responses;

[DebuggerDisplay("{Name}")]
public record RecipeResponse(
    Guid Id,
    string? Username,
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
    Guid? HeroMediaId);
