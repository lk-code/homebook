using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Entities;

[PrimaryKey(nameof(RecipeId), nameof(Position))]
[Table("RecipeSteps")]
public class RecipeStep
{
    [Required]
    public Guid RecipeId { get; set; }
    public virtual Recipe Recipe { get; set; } = null!;

    [Required]
    public int Position { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public int? TimerDurationInSeconds { get; set; }
}
