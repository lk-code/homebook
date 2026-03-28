using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Entities;

[PrimaryKey(nameof(RecipeId), nameof(MediaItemId))]
[Table("Recipe2MediaItems")]
public class Recipe2MediaItems
{
    [Required]
    public Guid RecipeId { get; set; }

    [ForeignKey(nameof(RecipeId))]
    public Recipe Recipe { get; set; } = null!;

    [Required]
    public Guid MediaItemId { get; set; }

    [ForeignKey(nameof(MediaItemId))]
    public MediaItem MediaItem { get; set; } = null!;

    [Required]
    public int Index { get; set; }
}
