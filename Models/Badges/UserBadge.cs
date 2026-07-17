using System.ComponentModel.DataAnnotations;
using StravaTeamApp.Models;

namespace StravaTeamApp.Models.Badges;

public class UserBadge
{
    public long Id { get; set; }

    public int BadgeId { get; set; }

    public Badge Badge { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public DateTime AwardedAtUtc { get; set; } = DateTime.UtcNow;

    public double AchievedValue { get; set; }

    public long TriggerActivityId { get; set; }

    public StravaActivity TriggerActivity { get; set; } = null!;
}