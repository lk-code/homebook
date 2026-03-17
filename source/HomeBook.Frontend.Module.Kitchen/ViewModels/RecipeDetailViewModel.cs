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
    public IList<IngredientViewModel> Ingredients { get; set; } = new List<IngredientViewModel>();
    public IList<StepViewModel> Steps { get; set; } = new List<StepViewModel>();
    public string Image { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public List<Guid> ImageMediaIds = [];

    public int NumberOfServings { get; set; }

    public bool HasAnnotations => Duration.HasValue || Servings.HasValue;

    public RecipeDetailViewModel()
    {
        NumberOfServings = Servings ?? 1;
    }
}
