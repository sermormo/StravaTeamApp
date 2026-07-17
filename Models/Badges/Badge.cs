using System.ComponentModel.DataAnnotations;

namespace StravaTeamApp.Models.Badges;

public class Badge
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string IconPath { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public BadgeRule Rule { get; set; } = null!;

    public ICollection<UserBadge> Awards { get; set; } =
        new List<UserBadge>();
}