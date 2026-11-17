using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("CustomElementValues")]
public class CustomElementValue
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string RecordKey { get; set; } = string.Empty;

    public int CustomElementMetadataId { get; set; }
    public virtual CustomElementMetadata? Metadata { get; set; }

    public string? ValueText { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? ValueNumber { get; set; }

    public DateTime? ValueDate { get; set; }

    public string? ValueJson { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

