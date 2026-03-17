using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace HomeBook.Backend.Data.Entities;

[DebuggerDisplay("[{nameof(StorageScopeRegistration)}] {Name}")]
[Table("StorageScopeRegistrations")]
public class StorageScopeRegistration
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid Id { get; set; }

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Scope Name must be between 1 and 50 characters long.")]
    [Required]
    public string Name { get; set; }

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Module Key must be between 1 and 50 characters long.")]
    [Required]
    public string ModuleKey { get; set; }

    public MediaItem MediaItem { get; set; }
}
