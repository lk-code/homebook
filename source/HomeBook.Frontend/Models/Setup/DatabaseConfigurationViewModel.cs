using System.ComponentModel.DataAnnotations;

namespace HomeBook.Frontend.Models.Setup;

public class DatabaseConfigurationViewModel
{
    [Required(ErrorMessage = "Database host is required")]
    [StringLength(255, ErrorMessage = "Host name cannot exceed 255 characters")]
    public string Host { get; set; } = string.Empty;

    [Required(ErrorMessage = "Port is required")]
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public string Port { get; set; } = "5432";

    [Required(ErrorMessage = "Database name is required")]
    [StringLength(64, ErrorMessage = "Database name cannot exceed 64 characters")]
    public string DatabaseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(64, ErrorMessage = "Username cannot exceed 64 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
    public string Password { get; set; } = string.Empty;

    public bool UseSSL { get; set; } = true;

    [Range(1, 300, ErrorMessage = "Connection timeout must be between 1 and 300 seconds")]
    public int? ConnectionTimeout { get; set; } = 30;
}
