using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace HomeBook.Backend.Data.Entities;

[DebuggerDisplay("[{nameof(MediaItem)}] {Name}")]
[Table("MediaItems")]
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
    [ForeignKey(nameof(User))]
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
}
