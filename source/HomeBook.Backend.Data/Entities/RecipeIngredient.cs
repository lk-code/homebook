using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeBook.Backend.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Entities;

[Index(nameof(NormalizedName), IsUnique = true)]
[Table("RecipeIngredients")]
public class RecipeIngredient : INormalizable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string NormalizedName { get; set; } = null!;

    public virtual ICollection<Recipe2RecipeIngredient> RecipeIngredient2Recipes { get; set; } =
        new List<Recipe2RecipeIngredient>();


    public void Normalize(IStringNormalizer normalizer)
    {
        NormalizedName = normalizer.Normalize(Name);
    }
}
