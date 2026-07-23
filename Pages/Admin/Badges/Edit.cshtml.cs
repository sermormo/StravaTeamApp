using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StravaTeamApp.Data;
using StravaTeamApp.Models.Badges;

namespace StravaTeamApp.Pages.Admin.Badges;

[Authorize(Roles = "Administrator")]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EditBadgeInput Input { get; set; } = new();

    public int AwardCount { get; private set; }

    public IReadOnlyList<SelectListItem> MetricOptions { get; } =
    [
        new("Distancia", ((int)BadgeMetric.Distance).ToString()),
        new("Elevación", ((int)BadgeMetric.ElevationGain).ToString()),
        new("Duración", ((int)BadgeMetric.Duration).ToString()),
        new(
            "Cantidad de actividades",
            ((int)BadgeMetric.ActivityCount).ToString())
    ];

    public IReadOnlyList<SelectListItem> OperationOptions { get; } =
    [
        new("Suma", ((int)BadgeOperation.Sum).ToString()),
        new("Conteo", ((int)BadgeOperation.Count).ToString()),
        new("Máximo", ((int)BadgeOperation.Maximum).ToString())
    ];

    public IReadOnlyList<SelectListItem> PeriodOptions { get; } =
    [
        new("Semanal", ((int)BadgePeriodType.Weekly).ToString()),
        new("Mensual", ((int)BadgePeriodType.Monthly).ToString()),
        new(
            "Personalizado",
            ((int)BadgePeriodType.Custom).ToString())
    ];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var badge = await _context.Badges
            .AsNoTracking()
            .Include(item => item.Rule)
            .SingleOrDefaultAsync(item => item.Id == id);

        if (badge is null)
        {
            return NotFound();
        }

        AwardCount = await _context.UserBadges
            .CountAsync(award => award.BadgeId == id);

        Input = new EditBadgeInput
        {
            Id = badge.Id,
            Name = badge.Name,
            Description = badge.Description,
            Metric = badge.Rule.Metric,
            Operation = badge.Rule.Operation,
            PeriodType = badge.Rule.PeriodType,
            TargetValue = badge.Rule.TargetValue,
            CustomStartDate = GetCustomStartDate(
                badge.Rule.CustomStartDateUtc),
            CustomEndDate = GetCustomEndDate(
                badge.Rule.CustomEndDateUtc)
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateRule();

        if (!ModelState.IsValid)
        {
            AwardCount = await _context.UserBadges
                .CountAsync(award => award.BadgeId == Input.Id);

            return Page();
        }

        var badge = await _context.Badges
            .Include(item => item.Rule)
            .SingleOrDefaultAsync(item => item.Id == Input.Id);

        if (badge is null)
        {
            return NotFound();
        }

        var customStartDateUtc = GetCustomStartDateUtc();
        var customEndDateUtc = GetCustomEndDateUtc();

        var ruleChanged = HasRuleChanged(
            badge.Rule,
            customStartDateUtc,
            customEndDateUtc);

        var removedAwardCount = 0;

        if (ruleChanged)
        {
            var awards = await _context.UserBadges
                .Where(award => award.BadgeId == badge.Id)
                .ToListAsync();

            removedAwardCount = awards.Count;
            AwardCount = removedAwardCount;

            _context.UserBadges.RemoveRange(awards);
        }
        else
        {
            AwardCount = await _context.UserBadges
                .CountAsync(award => award.BadgeId == badge.Id);
        }

        badge.Name = Input.Name.Trim();
        badge.Description = Input.Description.Trim();
        badge.IconPath = GetIconPath(Input.Metric);
        badge.UpdatedAtUtc = DateTime.UtcNow;

        badge.Rule.Metric = Input.Metric;
        badge.Rule.Operation = Input.Operation;
        badge.Rule.PeriodType = Input.PeriodType;
        badge.Rule.TargetValue = Input.TargetValue;
        badge.Rule.CustomStartDateUtc = customStartDateUtc;
        badge.Rule.CustomEndDateUtc = customEndDateUtc;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No fue posible actualizar la insignia.");

            return Page();
        }

        TempData["SuccessMessage"] = removedAwardCount switch
        {
            0 => "La insignia fue actualizada correctamente.",

            1 => "La insignia fue actualizada. Se eliminó 1 otorgamiento para evaluarlo nuevamente con el nuevo criterio.",

            _ => $"La insignia fue actualizada. Se eliminaron {removedAwardCount} otorgamientos para evaluarlos nuevamente con el nuevo criterio."
        };

        return RedirectToPage("./Index");
    }

    private void ValidateRule()
    {
        if (!Enum.IsDefined(Input.Metric))
        {
            ModelState.AddModelError(
                "Input.Metric",
                "Selecciona una métrica válida.");
        }

        if (!Enum.IsDefined(Input.Operation))
        {
            ModelState.AddModelError(
                "Input.Operation",
                "Selecciona una operación válida.");
        }

        if (!Enum.IsDefined(Input.PeriodType))
        {
            ModelState.AddModelError(
                "Input.PeriodType",
                "Selecciona un periodo válido.");
        }

        if (!double.IsFinite(Input.TargetValue) ||
            Input.TargetValue <= 0)
        {
            ModelState.AddModelError(
                "Input.TargetValue",
                "La meta debe ser mayor que cero.");
        }

        var countsActivities =
            Input.Metric == BadgeMetric.ActivityCount;

        var usesCountOperation =
            Input.Operation == BadgeOperation.Count;

        if (countsActivities != usesCountOperation)
        {
            ModelState.AddModelError(
                string.Empty,
                "Cantidad de actividades debe utilizar la operación Conteo.");
        }

        if (countsActivities &&
            Input.TargetValue != Math.Truncate(Input.TargetValue))
        {
            ModelState.AddModelError(
                "Input.TargetValue",
                "La cantidad de actividades debe ser un número entero.");
        }

        if (Input.PeriodType != BadgePeriodType.Custom)
        {
            return;
        }

        if (Input.CustomStartDate is null ||
            Input.CustomEndDate is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Debes indicar las fechas del periodo personalizado.");

            return;
        }

        if (Input.CustomEndDate < Input.CustomStartDate)
        {
            ModelState.AddModelError(
                "Input.CustomEndDate",
                "La fecha final debe ser igual o posterior a la fecha inicial.");
        }
    }

    private bool HasRuleChanged(
    BadgeRule currentRule,
    DateTime? customStartDateUtc,
    DateTime? customEndDateUtc)
    {
        return
            currentRule.Metric != Input.Metric ||
            currentRule.Operation != Input.Operation ||
            currentRule.PeriodType != Input.PeriodType ||
            currentRule.TargetValue != Input.TargetValue ||
            currentRule.CustomStartDateUtc != customStartDateUtc ||
            currentRule.CustomEndDateUtc != customEndDateUtc;
    }

    private static string GetIconPath(
        BadgeMetric metric)
    {
        return metric switch
        {
            BadgeMetric.Distance =>
                "/images/badges/distance.svg",

            BadgeMetric.ElevationGain =>
                "/images/badges/elevation.svg",

            BadgeMetric.Duration =>
                "/images/badges/duration.svg",

            BadgeMetric.ActivityCount =>
                "/images/badges/activity-count.svg",

            _ => throw new ArgumentOutOfRangeException(
                nameof(metric),
                metric,
                "Unsupported badge metric.")
        };
    }

    private DateTime? GetCustomStartDateUtc()
    {
        if (Input.PeriodType != BadgePeriodType.Custom ||
            Input.CustomStartDate is null)
        {
            return null;
        }

        return DateTime.SpecifyKind(
            Input.CustomStartDate.Value.ToDateTime(
                TimeOnly.MinValue),
            DateTimeKind.Utc);
    }

    private DateTime? GetCustomEndDateUtc()
    {
        if (Input.PeriodType != BadgePeriodType.Custom ||
            Input.CustomEndDate is null)
        {
            return null;
        }

        var exclusiveEndDate =
            Input.CustomEndDate.Value.AddDays(1);

        return DateTime.SpecifyKind(
            exclusiveEndDate.ToDateTime(
                TimeOnly.MinValue),
            DateTimeKind.Utc);
    }

    private static DateOnly? GetCustomStartDate(
        DateTime? customStartDateUtc)
    {
        return customStartDateUtc is null
            ? null
            : DateOnly.FromDateTime(
                customStartDateUtc.Value);
    }

    private static DateOnly? GetCustomEndDate(
        DateTime? customEndDateUtc)
    {
        return customEndDateUtc is null
            ? null
            : DateOnly.FromDateTime(
                customEndDateUtc.Value.AddDays(-1));
    }

    public sealed class EditBadgeInput
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(
            100,
            ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MaxLength(
            500,
            ErrorMessage = "La descripción no puede superar 500 caracteres.")]
        public string Description { get; set; } = string.Empty;

        public BadgeMetric Metric { get; set; }

        public BadgeOperation Operation { get; set; }

        public BadgePeriodType PeriodType { get; set; }

        public double TargetValue { get; set; }

        public DateOnly? CustomStartDate { get; set; }

        public DateOnly? CustomEndDate { get; set; }
    }
}
