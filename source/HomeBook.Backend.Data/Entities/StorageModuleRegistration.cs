using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace HomeBook.Backend.Data.Entities;

[DebuggerDisplay("[{nameof(StorageModuleRegistration)}] {Name}")]
[Table("StorageModuleRegistrations")]
public class StorageModuleRegistration
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public Guid Id { get; set; }

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Scope Name must be between 1 and 50 characters long.")]
    [Required]
    public string ScopeName { get; set; }

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Module Key must be between 1 and 50 characters long.")]
    [Required]
    public string ModuleKey { get; set; }
}
