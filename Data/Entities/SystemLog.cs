using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevExpressBlazorApp.Data.Entities;

[Table("SystemLogs")]
public class SystemLog
{
    [Key]
    public long Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(20)]
    public string LogLevel { get; set; } = "Error";

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }

    public string? StackTrace { get; set; }

    [MaxLength(200)]
    public string? Source { get; set; }

    [MaxLength(200)]
    public string? UserContext { get; set; }
}

