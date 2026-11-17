using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppLocations")]
public class AppLocation
{
    [Key]
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public virtual AppCompany? Company { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

