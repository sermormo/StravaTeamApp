using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StravaTeamApp.Data;
using StravaTeamApp.Models.Badges;

namespace StravaTeamApp.Pages.Admin.Badges;

[Authorize(Roles = "Administrator")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CreateBadgeInput Input { get; set; } = new();

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
        new("Personalizado", ((int)BadgePeriodType.Custom).ToString())
    ];

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateRule();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var badge = new Badge
        {
            Name = Input.Name.Trim(),
            Description = Input.Description.Trim(),
            IconPath = GetIconPath(Input.Metric),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,

            Rule = new BadgeRule
            {
                Metric = Input.Metric,
                Operation = Input.Operation,
                PeriodType = Input.PeriodType,
                TargetValue = Input.TargetValue,
                ActivityType = "Run",
                CustomStartDateUtc =
                    GetCustomStartDateUtc(),
                CustomEndDateUtc =
                    GetCustomEndDateUtc()
            }
        };

        _context.Badges.Add(badge);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            ModelState.AddModelError(
                string.Empty,
                "No fue posible guardar la insignia.");

            return Page();
        }

        TempData["SuccessMessage"] =
            "La insignia fue creada correctamente.";

        return RedirectToPage("./Index");
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

        if (Input.TargetValue <= 0)
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

    public sealed class CreateBadgeInput
    {
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