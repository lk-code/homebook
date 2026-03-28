using HomeBook.Client.Models;
using HomeBook.Frontend.Module.Kitchen.Models;
using HomeBook.Frontend.Module.Kitchen.ViewModels;

namespace HomeBook.Frontend.Module.Kitchen.Mappings;

public static class RecipeMappings
{
    public static RecipeDetailViewModel ToViewModel(this RecipeDetailDto r)
    {
        int? durationInMinutes = r.DurationWorkingMinutes
                                 + r.DurationCookingMinutes
                                 + r.DurationRestingMinutes;
        TimeSpan? duration = durationInMinutes.HasValue
            ? TimeSpan.FromMinutes(durationInMinutes.Value)
            : null;

        return new RecipeDetailViewModel
        {
            Id = r.Id,
            Username = r.Username,
            Name = r.Name,
            Description = r.Description,
            Servings = r.Servings,
            NumberOfServings = r.Servings ?? 1,
            CaloriesKcal = r.CaloriesKcal,
            Duration = duration,
            DurationWorkingMinutes = r.DurationWorkingMinutes.HasValue
                ? TimeSpan.FromMinutes(r.DurationWorkingMinutes.Value)
                : null,
            DurationCookingMinutes = r.DurationCookingMinutes.HasValue
                ? TimeSpan.FromMinutes(r.DurationCookingMinutes.Value)
                : null,
            DurationRestingMinutes = r.DurationRestingMinutes.HasValue
                ? TimeSpan.FromMinutes(r.DurationRestingMinutes.Value)
                : null,
            Ingredients = r.Ingredients
                .Select(x => new IngredientViewModel
                {
                    Name = x.Name,
                    Quantity = x.Quantity.HasValue ? Convert.ToDecimal(x.Quantity.Value) : null,
                    Unit = x.Unit,
                    AdditionalText = null
                })
                .ToList(),
            Steps = r.Steps
                .OrderBy(x => x.Position)
                .Select(x => new StepViewModel
                {
                    Description = x.Description,
                    TimerDurationInSeconds = x.TimerDurationInSeconds
                })
                .ToList(),
            Image = TestImageMappings.PlaceholderImage,
            Source = r.Source,
            Comments = r.Comments,
            ImageMediaIds = (r.ImageMediaItems.Length > 0
                    ? r.ImageMediaItems
                        .OrderBy(x => x.Index)
                        .Select(x => x.MediaItemId)
                    : r.ImageMediaIds)
                .ToList(),
            HeroMediaId = r.HeroMediaId
        };
    }

    public static RecipeViewModel ToViewModel(this RecipeDto r)
    {
        TimeSpan? duration = r.DurationInMinutes.HasValue
            ? TimeSpan.FromMinutes(r.DurationInMinutes.Value)
            : null;

        return new RecipeViewModel
        {
            Id = r.Id,
            Username = r.Username,
            Name = r.Name,
            Description = r.Description,
            Servings = r.Servings,
            CaloriesKcal = r.CaloriesKcal,
            Duration = duration,
            Ingredients = r.Ingredients,
            HeroMediaId = r.HeroMediaId,
        };
    }

    public static RecipeDto ToDto(this HomeBook.Client.Models.RecipeResponse r) =>
        new(
            r.Id!.Value,
            r.Username!,
            r.Name!,
            r.Description!,
            r.Servings,
            r.CaloriesKcal,
            r.DurationCookingMinutes,
            "",
            r.HeroMediaId);

    public static RecipeDetailDto ToDto(this HomeBook.Client.Models.RecipeDetailResponse r) =>
        new(
            r.Id!.Value,
            r.Username!,
            r.Name!,
            r.NormalizedName!,
            r.Description!,
            r.Servings,
            GetRecipeMediaItems(r),
            GetRecipeMediaItems(r).Select(x => x.MediaItemId).ToArray(),
            GetRecipeMediaItems(r).Select(x => (Guid?)x.MediaItemId).FirstOrDefault(),
            (r.Ingredients ?? []).Select(x => x.ToDto()).ToArray(),
            (r.Steps ?? []).Select(x => x.ToDto()).ToArray(),
            r.DurationWorkingMinutes,
            r.DurationCookingMinutes,
            r.DurationRestingMinutes,
            r.CaloriesKcal,
            r.Comments!,
            r.Source!);

    public static RecipeMediaItemDto ToDto(this HomeBook.Client.Models.RecipeMediaItemResponse r) =>
        new(r.MediaItemId!.Value,
            r.Index!.Value);

    public static RecipeIngredientDto ToDto(this RecipeIngredientResponse r) =>
        new(r.Name!,
            r.Quantity,
            r.Unit);

    public static RecipeStepDto ToDto(this RecipeStepResponse r) =>
        new(r.Description!,
            r.Position!.Value,
            r.TimerDurationInSeconds);

    public static CreateRecipeMediaItemRequest ToRequest(this RecipeMediaItemDto dto) =>
        new()
        {
            MediaItemId = dto.MediaItemId,
            Index = dto.Index
        };

    public static CreateRecipeIngredientRequest ToRequest(this RecipeIngredientDto dto) =>
        new()
        {
            Name = dto.Name,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
        };

    public static CreateRecipeStepRequest ToRequest(this RecipeStepDto dto) =>
        new()
        {
            Description = dto.Description,
            Position = dto.Position,
            TimerDurationInSeconds = dto.TimerDurationInSeconds
        };

    public static RecipeIngredientDto ToDto(this IngredientViewModel dto) =>
        new(dto.Name,
            dto.Quantity.HasValue ? Convert.ToDouble(dto.Quantity.Value) : null,
            dto.Unit);

    public static RecipeStepDto ToDto(this StepViewModel dto,
        int position) =>
        new(dto.Description,
            position,
            dto.TimerDurationInSeconds);

    private static RecipeMediaItemDto[] GetRecipeMediaItems(this HomeBook.Client.Models.RecipeDetailResponse response)
    {
        RecipeMediaItemDto[] mediaItems = (response.MediaItems ?? [])
            .Where(x => x.MediaItemId.HasValue && x.Index.HasValue)
            .Select(x => x.ToDto())
            .OrderBy(x => x.Index)
            .ToArray();
        if (mediaItems.Length > 0)
            return mediaItems;

        return (response.MediaIds ?? [])
            .Where(x => x.HasValue)
            .Select((x, index) => new RecipeMediaItemDto(x!.Value, index))
            .ToArray();
    }
}
