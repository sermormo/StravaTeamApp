using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StravaTeamApp.Models;

public sealed class SystemLog
{
    [Key]
    public int Id { get; set; }

    [Column("Fecha")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(20)]
    [Column("Nivel")]
    public string Level { get; set; } =
        SystemLogLevels.Information;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } =
        SystemLogCategories.System;

    [Required]
    [MaxLength(100)]
    public string EventName { get; set; } =
        "LegacyEvent";

    [Required]
    [MaxLength(500)]
    [Column("Mensaje")]
    public string Message { get; set; } =
        "Evento sin descripción.";

    [Required]
    [MaxLength(4000)]
    [Column("Detalles")]
    public string Details { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? ActorUserId { get; set; }

    [MaxLength(100)]
    public string? RequestId { get; set; }

    [MaxLength(300)]
    public string? RequestPath { get; set; }

    // Temporary compatibility aliases for existing code.
    [NotMapped]
    public DateTime Fecha
    {
        get => CreatedAtUtc;
        set => CreatedAtUtc = value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }

    [NotMapped]
    public string Nivel
    {
        get => Level;
        set => Level = string.IsNullOrWhiteSpace(value)
            ? SystemLogLevels.Information
            : value;
    }

    [NotMapped]
    public string Mensaje
    {
        get => Message;
        set => Message = string.IsNullOrWhiteSpace(value)
            ? "Evento sin descripción."
            : value;
    }

    [NotMapped]
    public string Detalles
    {
        get => Details;
        set => Details = value ?? string.Empty;
    }
}
