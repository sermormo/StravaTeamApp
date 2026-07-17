using System.ComponentModel.DataAnnotations;

namespace StravaTeamApp.Models.Badges;

public class BadgeRule
{
    public int BadgeId { get; set; }

    public Badge Badge { get; set; } = null!;

    public BadgeMetric Metric { get; set; }

    public BadgeOperation Operation { get; set; }

    public BadgePeriodType PeriodType { get; set; }

    public double TargetValue { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActivityType { get; set; } = "Run";

    public DateTime? CustomStartDateUtc { get; set; }

    public DateTime? CustomEndDateUtc { get; set; }
}