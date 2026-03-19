using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace HomeBook.Backend.Data.Entities;

[DebuggerDisplay("[{nameof(MediaItem)}] {Name}")]
[Table("MediaItems")]
[Index(nameof(StorageScopeId), nameof(FileName), IsUnique = true)]
public class MediaItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// the storage scope entity id
    /// </summary>
    [Required]
    public Guid StorageScopeId { get; set; }

    /// <summary>
    /// the storage scope entity
    /// </summary>
    public virtual StorageScopeRegistration? StorageScope { get; set; }

    /// <summary>
    /// the file-name
    /// </summary>
    [Required]
    [StringLength(250, MinimumLength = 1, ErrorMessage = "FileName must be between 1 and 250 characters long.")]
    public required string FileName { get; set; }

    public virtual ICollection<Recipe2MediaItems> MediaItem2Recipes { get; set; } =
        new List<Recipe2MediaItems>();
}
