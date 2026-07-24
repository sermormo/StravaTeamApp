using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Data;
using StravaTeamApp.Models;
using StravaTeamApp.Models.Badges;

namespace StravaTeamApp.Services;

public sealed class BadgeEvaluationService
{
    private readonly AppDbContext _context;

    public BadgeEvaluationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> EvaluateUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A user identifier is required.",
                nameof(userId));
        }

        var badges = await _context.Badges
            .AsNoTracking()
            .Include(badge => badge.Rule)
            .Where(badge => badge.IsActive)
            .ToListAsync(cancellationToken);

        if (badges.Count == 0)
        {
            return 0;
        }

        var activities = await _context.Activities
            .AsNoTracking()
            .Where(activity => activity.UserId == userId)
            .OrderBy(activity => activity.StartDate)
            .ThenBy(activity => activity.Id)
            .ToListAsync(cancellationToken);

        if (activities.Count == 0)
        {
            return 0;
        }

        var activeBadgeIds = badges
            .Select(badge => badge.Id)
            .ToList();

        var existingAwards = await _context.UserBadges
            .AsNoTracking()
            .Where(award =>
                award.UserId == userId &&
                activeBadgeIds.Contains(award.BadgeId))
            .Select(award => new AwardKey(
                award.BadgeId,
                award.PeriodStartUtc,
                award.PeriodEndUtc))
            .ToListAsync(cancellationToken);

        var awardKeys = existingAwards.ToHashSet();
        var newAwards = new List<UserBadge>();

        foreach (var badge in badges)
        {
            var eligibleActivities = activities
                .Where(activity => string.Equals(
                    activity.Type,
                    badge.Rule.ActivityType,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var period in GetPeriods(
                badge.Rule,
                eligibleActivities))
            {
                var awardKey = new AwardKey(
                    badge.Id,
                    period.StartUtc,
                    period.EndUtc);

                if (awardKeys.Contains(awardKey))
                {
                    continue;
                }

                var result = EvaluatePeriod(
                    badge.Rule,
                    period.Activities);

                if (result is null)
                {
                    continue;
                }

                newAwards.Add(new UserBadge
                {
                    BadgeId = badge.Id,
                    UserId = userId,
                    PeriodStartUtc = period.StartUtc,
                    PeriodEndUtc = period.EndUtc,
                    AwardedAtUtc = DateTime.UtcNow,
                    AchievedValue = result.AchievedValue,
                    TriggerActivityId =
                        result.TriggerActivity.Id
                });

                awardKeys.Add(awardKey);
            }
        }

        if (newAwards.Count == 0)
        {
            return 0;
        }

        _context.UserBadges.AddRange(newAwards);

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            foreach (var award in newAwards)
            {
                _context.Entry(award).State =
                    EntityState.Detached;
            }

            throw;
        }

        return newAwards.Count;
    }

    private static IReadOnlyList<EvaluationPeriod>
        GetPeriods(
            BadgeRule rule,
            IReadOnlyList<StravaActivity> activities)
    {
        return rule.PeriodType switch
        {
            BadgePeriodType.Weekly =>
                activities
                    .GroupBy(activity =>
                        GetWeekStartUtc(
                            activity.StartDate))
                    .Select(group =>
                        new EvaluationPeriod(
                            group.Key,
                            group.Key.AddDays(7),
                            group
                                .OrderBy(activity =>
                                    activity.StartDate)
                                .ThenBy(activity =>
                                    activity.Id)
                                .ToList()))
                    .ToList(),

            BadgePeriodType.Monthly =>
                activities
                    .GroupBy(activity =>
                        GetMonthStartUtc(
                            activity.StartDate))
                    .Select(group =>
                        new EvaluationPeriod(
                            group.Key,
                            group.Key.AddMonths(1),
                            group
                                .OrderBy(activity =>
                                    activity.StartDate)
                                .ThenBy(activity =>
                                    activity.Id)
                                .ToList()))
                    .ToList(),

            BadgePeriodType.Custom =>
                GetCustomPeriod(rule, activities),

            _ => throw new InvalidOperationException(
                $"Unsupported badge period: {rule.PeriodType}.")
        };
    }

    private static IReadOnlyList<EvaluationPeriod>
        GetCustomPeriod(
            BadgeRule rule,
            IReadOnlyList<StravaActivity> activities)
    {
        if (rule.CustomStartDateUtc is null ||
            rule.CustomEndDateUtc is null ||
            rule.CustomEndDateUtc <=
                rule.CustomStartDateUtc)
        {
            return [];
        }

        var startUtc = rule.CustomStartDateUtc.Value;
        var endUtc = rule.CustomEndDateUtc.Value;

        var periodActivities = activities
            .Where(activity =>
                activity.StartDate >= startUtc &&
                activity.StartDate < endUtc)
            .OrderBy(activity => activity.StartDate)
            .ThenBy(activity => activity.Id)
            .ToList();

        return periodActivities.Count == 0
            ? []
            :
            [
                new EvaluationPeriod(
                    startUtc,
                    endUtc,
                    periodActivities)
            ];
    }

    private static EvaluationResult? EvaluatePeriod(
        BadgeRule rule,
        IReadOnlyList<StravaActivity> activities)
    {
        return rule.Operation switch
        {
            BadgeOperation.Sum =>
                EvaluateSum(rule, activities),

            BadgeOperation.Count =>
                EvaluateCount(rule, activities),

            BadgeOperation.Maximum =>
                EvaluateMaximum(rule, activities),

            _ => throw new InvalidOperationException(
                $"Unsupported badge operation: {rule.Operation}.")
        };
    }

    private static EvaluationResult? EvaluateSum(
        BadgeRule rule,
        IReadOnlyList<StravaActivity> activities)
    {
        var accumulatedValue = 0.0;

        foreach (var activity in activities)
        {
            accumulatedValue += GetMetricValue(
                rule.Metric,
                activity);

            if (accumulatedValue >= rule.TargetValue)
            {
                return new EvaluationResult(
                    activity,
                    accumulatedValue);
            }
        }

        return null;
    }

    private static EvaluationResult? EvaluateCount(
        BadgeRule rule,
        IReadOnlyList<StravaActivity> activities)
    {
        if (rule.Metric != BadgeMetric.ActivityCount)
        {
            return null;
        }

        for (var index = 0;
             index < activities.Count;
             index++)
        {
            var achievedValue = index + 1;

            if (achievedValue >= rule.TargetValue)
            {
                return new EvaluationResult(
                    activities[index],
                    achievedValue);
            }
        }

        return null;
    }

    private static EvaluationResult? EvaluateMaximum(
        BadgeRule rule,
        IReadOnlyList<StravaActivity> activities)
    {
        foreach (var activity in activities)
        {
            var value = GetMetricValue(
                rule.Metric,
                activity);

            if (value >= rule.TargetValue)
            {
                return new EvaluationResult(
                    activity,
                    value);
            }
        }

        return null;
    }

    private static double GetMetricValue(
        BadgeMetric metric,
        StravaActivity activity)
    {
        return metric switch
        {
            BadgeMetric.Distance =>
                activity.Distance / 1000.0,

            BadgeMetric.ElevationGain =>
                activity.TotalElevationGain,

            BadgeMetric.Duration =>
                activity.MovingTime / 60.0,

            BadgeMetric.ActivityCount => 1,

            _ => throw new InvalidOperationException(
                $"Unsupported badge metric: {metric}.")
        };
    }

    private static DateTime GetWeekStartUtc(
        DateTime value)
    {
        var date = value.Date;

        var daysSinceMonday =
            ((int)date.DayOfWeek + 6) % 7;

        return DateTime.SpecifyKind(
            date.AddDays(-daysSinceMonday),
            DateTimeKind.Utc);
    }

    private static DateTime GetMonthStartUtc(
        DateTime value)
    {
        return new DateTime(
            value.Year,
            value.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }

    private sealed record EvaluationPeriod(
        DateTime StartUtc,
        DateTime EndUtc,
        IReadOnlyList<StravaActivity> Activities);

    private sealed record EvaluationResult(
        StravaActivity TriggerActivity,
        double AchievedValue);

    private sealed record AwardKey(
        int BadgeId,
        DateTime PeriodStartUtc,
        DateTime PeriodEndUtc);
}