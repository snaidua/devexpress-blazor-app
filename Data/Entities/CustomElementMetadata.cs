using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("CustomElementMetadata")]
public class CustomElementMetadata
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = "Customer";

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string DisplayLabel { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string FieldType { get; set; } = "Text"; // Text, Number, Date, Dropdown, Boolean

    public bool IsRequired { get; set; } = false;

    [MaxLength(2000)]
    public string? DropdownOptions { get; set; } // Comma-delimited or JSON array: "High,Medium,Low"

    [MaxLength(500)]
    public string? DefaultValue { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<CustomElementValue> Values { get; set; } = new List<CustomElementValue>();
}

