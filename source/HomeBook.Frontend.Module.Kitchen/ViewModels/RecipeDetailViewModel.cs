namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public class RecipeDetailViewModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? Servings { get; set; }
    public int? CaloriesKcal { get; set; }
    public TimeSpan? Duration { get; set; }
    public TimeSpan? DurationWorkingMinutes { get; set; }
    public TimeSpan? DurationCookingMinutes { get; set; }
    public TimeSpan? DurationRestingMinutes { get; set; }
    public List<Guid> ImageMediaIds { get; set; } = [];
    public List<MediaItemViewModel> ImageMediaItems { get; set; } = [];
    public List<IngredientViewModel> Ingredients { get; set; } = [];
    public List<StepViewModel> Steps { get; set; } = [];
    public string Image { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public Uri? HeroImageUri { get; set; }
    public Guid? HeroMediaId { get; set; }

    public int NumberOfServings { get; set; }

    public bool HasAnnotations => Duration.HasValue || Servings.HasValue;

    public RecipeDetailViewModel()
    {
        NumberOfServings = Servings ?? 1;
    }
}
