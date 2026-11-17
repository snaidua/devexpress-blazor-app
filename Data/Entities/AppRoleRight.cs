using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppRoleRights")]
public class AppRoleRight
{
    [Key]
    public int Id { get; set; }

    public int RoleId { get; set; }
    public virtual AppRole? Role { get; set; }

    public int PageId { get; set; }
    public virtual AppPage? Page { get; set; }

    public bool CanView { get; set; } = true;
    public bool CanCreate { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
    public bool CanExport { get; set; } = false;
}

