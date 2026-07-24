using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StravaTeamApp.Data;
using StravaTeamApp.Models;
using StravaTeamApp.Services;

namespace StravaTeamApp.Pages.Sistema;

[Authorize(Roles = "Administrator")]
public class LogsModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public List<SystemLog> Logs { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Level { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<string> LevelOptions =>
        SystemLogLevels.All;

    public IReadOnlyList<string> CategoryOptions =>
        SystemLogCategories.All;

    public LogsModel(
        AppDbContext context,
        ISystemLogService systemLogService)
    {
        _context = context;
        _systemLogService = systemLogService;
    }

    public async Task OnGetAsync()
    {
        var query = _context.SystemLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Level))
        {
            query = query.Where(log =>
                log.Level == Level);
        }

        if (!string.IsNullOrWhiteSpace(Category))
        {
            query = query.Where(log =>
                log.Category == Category);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var searchTerm = Search.Trim();

            query = query.Where(log =>
                log.Message.Contains(searchTerm) ||
                log.Details.Contains(searchTerm) ||
                log.EventName.Contains(searchTerm) ||
                (log.ActorUserId != null &&
                    log.ActorUserId.Contains(searchTerm)));
        }

        Logs = await query
            .OrderByDescending(log =>
                log.CreatedAtUtc)
            .Take(200)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostClearAsync()
    {
        try
        {
            var deletedCount =
                await _context.SystemLogs
                    .ExecuteDeleteAsync();

            await _systemLogService.InformationAsync(
                category:
                    SystemLogCategories.Administration,
                eventName: "SystemLogsCleared",
                message:
                    "Un administrador vació la bitácora del sistema.",
                details:
                    $"DeletedCount={deletedCount}");

            TempData["SuccessMessage"] =
                $"Se eliminaron {deletedCount} registros. " +
                "La operación quedó registrada.";
        }
        catch (Exception exception)
        {
            await _systemLogService.ErrorAsync(
                category:
                    SystemLogCategories.Administration,
                eventName: "SystemLogsClearFailed",
                message:
                    "No fue posible vaciar la bitácora del sistema.",
                exception: exception);

            TempData["ErrorMessage"] =
                "No fue posible vaciar la bitácora.";
        }

        return RedirectToPage();
    }

    public string GetLevelLabel(string level)
    {
        return level switch
        {
            SystemLogLevels.Information =>
                "Información",
            SystemLogLevels.Warning =>
                "Advertencia",
            SystemLogLevels.Error =>
                "Error",
            SystemLogLevels.Critical =>
                "Crítico",
            "Info" => "Información",
            _ => level
        };
    }

    public string GetCategoryLabel(string category)
    {
        return category switch
        {
            SystemLogCategories.System =>
                "Sistema",
            SystemLogCategories.Authentication =>
                "Autenticación",
            SystemLogCategories.Authorization =>
                "Autorización",
            SystemLogCategories.Strava =>
                "Strava",
            SystemLogCategories.Activities =>
                "Actividades",
            SystemLogCategories.Badges =>
                "Insignias",
            SystemLogCategories.Administration =>
                "Administración",
            _ => category
        };
    }
}
