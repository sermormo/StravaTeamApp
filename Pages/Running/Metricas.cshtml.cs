using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Data;
using StravaTeamApp.Models;
using StravaTeamApp.Models.Badges;
using StravaTeamApp.Services;

namespace StravaTeamApp.Pages;

[Authorize]
public class MetricasModel : PageModel
{
    private readonly StravaService _stravaService;
    private readonly BadgeEvaluationService
        _badgeEvaluationService;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser>
        _userManager;

    public List<ConsolidatedSummary> ConsolidatedData
        { get; set; } = new();

    public List<StravaActivity> DetailedActivities
        { get; set; } = new();

    public List<BadgeAwardSummary> BadgeAwards
        { get; set; } = new();

    public MetricasModel(
        StravaService stravaService,
        BadgeEvaluationService badgeEvaluationService,
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _stravaService = stravaService;
        _badgeEvaluationService = badgeEvaluationService;
        _context = context;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return;
        }

        string? accessToken;

        try
        {
            accessToken =
                await _stravaService.GetValidAccessTokenAsync(
                    user,
                    HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            accessToken = null;

            _context.SystemLogs.Add(new SystemLog
            {
                Nivel = "Error",
                Mensaje =
                    "No fue posible renovar el acceso a Strava.",
                Detalles =
                    $"Usuario: {user.Id} | Error: {ex.Message}"
            });

            await _context.SaveChangesAsync();
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            _context.SystemLogs.Add(new SystemLog
            {
                Nivel = "Warning",
                Mensaje = "Token vacío",
                Detalles =
                    $"El usuario {user.Email} intentó cargar " +
                    "la página sin un token válido."
            });

            await _context.SaveChangesAsync();
        }
        else
        {
            try
            {
                var apiActivities =
                    await _stravaService
                        .GetAthleteActivitiesAsync(
                            accessToken);

                var displayName =
                    $"{user.Nombre} {user.Apellido}".Trim();

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName =
                        user.Email ?? "Corredor desconocido";
                }

                var activityIds = apiActivities
                    .Select(activity => activity.Id)
                    .ToList();

                var existingActivities =
                    await _context.Activities
                        .Where(activity =>
                            activityIds.Contains(activity.Id))
                        .ToDictionaryAsync(
                            activity => activity.Id);

                foreach (var activity in apiActivities)
                {
                    activity.UserId = user.Id;
                    activity.AthleteName = displayName;

                    if (existingActivities.TryGetValue(
                        activity.Id,
                        out var existingActivity))
                    {
                        existingActivity.Name =
                            activity.Name;

                        existingActivity.Distance =
                            activity.Distance;

                        existingActivity.TotalElevationGain =
                            activity.TotalElevationGain;

                        existingActivity.MovingTime =
                            activity.MovingTime;

                        existingActivity.Type =
                            activity.Type;

                        existingActivity.StartDate =
                            activity.StartDate;

                        existingActivity.UserId =
                            user.Id;

                        existingActivity.AthleteName =
                            displayName;
                    }
                    else
                    {
                        _context.Activities.Add(activity);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _context.SystemLogs.Add(new SystemLog
                {
                    Nivel = "Error",
                    Mensaje =
                        "No fue posible sincronizar las actividades.",
                    Detalles =
                        $"Usuario: {user.Id} | Error: {ex.Message}"
                });

                await _context.SaveChangesAsync();
            }
        }

        try
        {
            await _badgeEvaluationService.EvaluateUserAsync(
                user.Id,
                HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _context.SystemLogs.Add(new SystemLog
            {
                Nivel = "Error",
                Mensaje =
                    "No fue posible evaluar las insignias.",
                Detalles =
                    $"Usuario: {user.Id} | Error: {ex.Message}"
            });

            await _context.SaveChangesAsync();
        }

        var badgeAwards =
            await _context.UserBadges
                .AsNoTracking()
                .Include(award => award.Badge)
                    .ThenInclude(badge => badge.Rule)
                .Include(award =>
                    award.TriggerActivity)
                .OrderByDescending(award =>
                    award.TriggerActivity.StartDate)
                .ThenBy(award =>
                    award.Badge.Name)
                .ToListAsync();

        BadgeAwards = badgeAwards
            .Select(award => new BadgeAwardSummary
            {
                Id = award.Id,
                BadgeId = award.BadgeId,
                UserId = award.UserId,
                BadgeName = award.Badge.Name,
                BadgeDescription =
                    award.Badge.Description,
                IconPath = award.Badge.IconPath,
                IsActive = award.Badge.IsActive,
                Metric = award.Badge.Rule.Metric,
                PeriodStartUtc =
                    award.PeriodStartUtc,
                PeriodEndUtc =
                    award.PeriodEndUtc,
                AwardedAtUtc =
                    award.AwardedAtUtc,
                AchievedValue =
                    award.AchievedValue,
                TriggerActivityId =
                    award.TriggerActivityId,
                TriggerActivityName =
                    award.TriggerActivity.Name,
                TriggerActivityDate =
                    award.TriggerActivity.StartDate,
                TriggerDistanceKm = Math.Round(
                    award.TriggerActivity.Distance /
                    1000.0,
                    2),
                TriggerElevationMeters = Math.Round(
                    award.TriggerActivity
                        .TotalElevationGain,
                    2),
                TriggerTimeMinutes = Math.Round(
                    award.TriggerActivity.MovingTime /
                    60.0,
                    2)
            })
            .ToList();

        DetailedActivities =
            await _context.Activities
                .AsNoTracking()
                .OrderByDescending(activity =>
                    activity.StartDate)
                .ToListAsync();

        ConsolidatedData = DetailedActivities
            .GroupBy(activity => new
            {
                activity.UserId,
                activity.AthleteName,
                Year = activity.StartDate.Year,
                Month = activity.StartDate.Month
            })
            .Select(group => new ConsolidatedSummary
            {
                UserId = group.Key.UserId,
                AthleteName =
                    group.Key.AthleteName,
                MonthYear =
                    $"{group.Key.Year}-" +
                    $"{group.Key.Month:D2}",
                TotalActivities =
                    group.Count(),
                TotalDistanceKm = Math.Round(
                    group.Sum(activity =>
                        activity.Distance) / 1000.0,
                    2),
                TotalTimeMin = Math.Round(
                    group.Sum(activity =>
                        activity.MovingTime) / 60.0,
                    2)
            })
            .OrderByDescending(summary =>
                summary.TotalDistanceKm)
            .ThenBy(summary =>
                summary.AthleteName)
            .ToList();
    }

    public IReadOnlyList<BadgeAwardSummary>
        GetBadgeAwardsForUser(string userId)
    {
        return BadgeAwards
            .Where(award => string.Equals(
                award.UserId,
                userId,
                StringComparison.Ordinal))
            .OrderByDescending(award =>
                award.TriggerActivityDate)
            .ToList();
    }

    public IReadOnlyList<BadgeAwardSummary>
        GetBadgeAwardsForMonth(
            string userId,
            string monthYear)
    {
        return BadgeAwards
            .Where(award =>
                string.Equals(
                    award.UserId,
                    userId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    award.MonthYear,
                    monthYear,
                    StringComparison.Ordinal))
            .OrderBy(award =>
                award.TriggerActivityDate)
            .ThenBy(award =>
                award.BadgeName)
            .ToList();
    }
}

public class ConsolidatedSummary
{
    public string UserId { get; set; } =
        string.Empty;

    public string AthleteName { get; set; } =
        string.Empty;

    public string MonthYear { get; set; } =
        string.Empty;

    public int TotalActivities { get; set; }

    public double TotalDistanceKm { get; set; }

    public double TotalTimeMin { get; set; }
}

public class BadgeAwardSummary
{
    public long Id { get; set; }

    public int BadgeId { get; set; }

    public string UserId { get; set; } =
        string.Empty;

    public string BadgeName { get; set; } =
        string.Empty;

    public string BadgeDescription { get; set; } =
        string.Empty;

    public string IconPath { get; set; } =
        string.Empty;

    public bool IsActive { get; set; }

    public BadgeMetric Metric { get; set; }

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public DateTime AwardedAtUtc { get; set; }

    public double AchievedValue { get; set; }

    public long TriggerActivityId { get; set; }

    public string TriggerActivityName { get; set; } =
        string.Empty;

    public DateTime TriggerActivityDate { get; set; }

    public double TriggerDistanceKm { get; set; }

    public double TriggerElevationMeters { get; set; }

    public double TriggerTimeMinutes { get; set; }

    public string MonthYear =>
        TriggerActivityDate.ToString("yyyy-MM");

    public DateTime PeriodEndDisplay =>
        PeriodEndUtc > PeriodStartUtc
            ? PeriodEndUtc.AddTicks(-1)
            : PeriodEndUtc;

    public string AchievedValueDisplay =>
        Metric switch
        {
            BadgeMetric.Distance =>
                $"{AchievedValue:0.##} km",

            BadgeMetric.ElevationGain =>
                $"{AchievedValue:0.##} m",

            BadgeMetric.Duration =>
                $"{AchievedValue:0.##} min",

            BadgeMetric.ActivityCount =>
                $"{AchievedValue:0} actividades",

            _ => AchievedValue.ToString("0.##")
        };
}