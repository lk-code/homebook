namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public class RecipeViewModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? Servings { get; set; }
    public int? CaloriesKcal { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Ingredients { get; set; } = string.Empty;
    public Uri? HeroImageUri { get; set; }
    public Guid? HeroMediaId { get; set; }

    public bool HasAnnotations => Duration.HasValue || Servings.HasValue;
}
