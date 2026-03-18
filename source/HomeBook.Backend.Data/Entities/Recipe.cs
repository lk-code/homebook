using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using HomeBook.Backend.Abstractions.Contracts;

namespace HomeBook.Backend.Data.Entities;

[DebuggerDisplay("[{nameof(Recipe)}] {Name}")]
[Table("Recipes")]
public class Recipe : INormalizable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters long.")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public string NormalizedName { get; set; } = null!;

    public string? Description { get; set; }

    public int? DurationWorkingMinutes { get; set; }

    public int? DurationCookingMinutes { get; set; }

    public int? DurationRestingMinutes { get; set; }

    public int? CaloriesKcal { get; set; }

    public int? Servings { get; set; }

    public string? Comments { get; set; }

    public string? Source { get; set; }

    [ForeignKey(nameof(User))]
    public Guid? UserId { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<Recipe2RecipeIngredient> Recipe2RecipeIngredient { get; set; } = new List<Recipe2RecipeIngredient>();

    public virtual ICollection<Recipe2MediaItems> Recipe2MediaItem { get; set; } = new List<Recipe2MediaItems>();

    public virtual ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();

    public void Normalize(IStringNormalizer normalizer)
    {
        NormalizedName = normalizer.Normalize(Name);
    }
}
