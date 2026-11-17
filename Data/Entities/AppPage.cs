using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppPages")]
public class AppPage
{
    [Key]
    public int Id { get; set; }

    public int? ParentId { get; set; }
    public virtual AppPage? Parent { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Route { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Icon { get; set; } = "grid";

    [MaxLength(50)]
    public string? BadgeText { get; set; }

    [MaxLength(50)]
    public string? BadgeColor { get; set; } = "bg-primary";

    [MaxLength(500)]
    public string? BadgeDapperQuery { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<AppPage> ChildPages { get; set; } = new List<AppPage>();
    public virtual ICollection<AppRoleRight> RoleRights { get; set; } = new List<AppRoleRight>();
}

