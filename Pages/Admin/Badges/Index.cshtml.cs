using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Data;
using StravaTeamApp.Models.Badges;

namespace StravaTeamApp.Pages.Admin.Badges;

[Authorize(Roles = "Administrator")]
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public List<BadgeSummary> Badges { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var badgeEntities = await _context.Badges
            .AsNoTracking()
            .Include(badge => badge.Rule)
            .OrderByDescending(badge => badge.IsActive)
            .ThenBy(badge => badge.Name)
            .ToListAsync();

        Badges = badgeEntities
            .Select(badge => new BadgeSummary
            {
                Id = badge.Id,
                Name = badge.Name,
                Description = badge.Description,
                IconPath = badge.IconPath,
                Metric = GetMetricLabel(badge.Rule.Metric),
                Operation = GetOperationLabel(
                    badge.Rule.Operation),
                Period = GetPeriodLabel(
                    badge.Rule.PeriodType),
                Target = FormatTarget(badge.Rule),
                IsActive = badge.IsActive
            })
            .ToList();
    }

    private static string GetMetricLabel(
        BadgeMetric metric)
    {
        return metric switch
        {
            BadgeMetric.Distance => "Distancia",
            BadgeMetric.ElevationGain => "Elevación",
            BadgeMetric.Duration => "Duración",
            BadgeMetric.ActivityCount =>
                "Cantidad de actividades",
            _ => "Desconocida"
        };
    }

    private static string GetOperationLabel(
        BadgeOperation operation)
    {
        return operation switch
        {
            BadgeOperation.Sum => "Suma",
            BadgeOperation.Count => "Conteo",
            BadgeOperation.Maximum => "Máximo",
            _ => "Desconocida"
        };
    }

    private static string GetPeriodLabel(
        BadgePeriodType periodType)
    {
        return periodType switch
        {
            BadgePeriodType.Weekly => "Semanal",
            BadgePeriodType.Monthly => "Mensual",
            BadgePeriodType.Custom => "Personalizado",
            _ => "Desconocido"
        };
    }

    private static string FormatTarget(
        BadgeRule rule)
    {
        var unit = rule.Metric switch
        {
            BadgeMetric.Distance => "km",
            BadgeMetric.ElevationGain => "m",
            BadgeMetric.Duration => "min",
            BadgeMetric.ActivityCount => "actividades",
            _ => string.Empty
        };

        return $"{rule.TargetValue:0.##} {unit}".Trim();
    }

    public sealed class BadgeSummary
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } =
            string.Empty;

        public string IconPath { get; init; } =
            string.Empty;

        public string Metric { get; init; } =
            string.Empty;

        public string Operation { get; init; } =
            string.Empty;

        public string Period { get; init; } =
            string.Empty;

        public string Target { get; init; } =
            string.Empty;

        public bool IsActive { get; init; }
    }
}