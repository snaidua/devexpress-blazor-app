using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppUserPreferences")]
public class AppUserPreference
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public virtual AppUser? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string ThemeName { get; set; } = "blazing-berry";

    public bool CompactMode { get; set; } = false;

    public bool SidebarCollapsed { get; set; } = false;

    [MaxLength(20)]
    public string Language { get; set; } = "en-US";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

