using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppRoles")]
public class AppRole
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    public bool IsSystem { get; set; } = false;

    public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public virtual ICollection<AppRoleRight> RoleRights { get; set; } = new List<AppRoleRight>();
}

