using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppUsers")]
public class AppUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public virtual AppRole? Role { get; set; }

    public int? DefaultCompanyId { get; set; }
    public virtual AppCompany? DefaultCompany { get; set; }

    public int? DefaultLocationId { get; set; }
    public virtual AppLocation? DefaultLocation { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public virtual AppUserPreference? Preference { get; set; }
}

