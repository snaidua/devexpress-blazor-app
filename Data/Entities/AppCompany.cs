using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("AppCompanies")]
public class AppCompany
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string TaxId { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public bool IsActive { get; set; } = true;

    public virtual ICollection<AppLocation> Locations { get; set; } = new List<AppLocation>();
}

