namespace HomeBook.Frontend.Module.Kitchen.ViewModels;

public class RecipeSearchResultViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Uri? HeroImageUri { get; set; }
    public Guid? HeroMediaId { get; set; }
}
