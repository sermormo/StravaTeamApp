using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StravaTeamApp.Services;
using StravaTeamApp.Data;
using StravaTeamApp.Models;

namespace StravaTeamApp.Pages;

[Authorize]
public class MetricasModel : PageModel
{
    private readonly StravaAthleteService _stravaService; // Usamos el nuevo servicio
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public List<ConsolidatedSummary> ConsolidatedData { get; set; } = new();
    public List<StravaActivity> DetailedActivities { get; set; } = new();

    public MetricasModel(
        StravaAthleteService stravaService,
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _stravaService = stravaService;
        _context = context;
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return;

        string? accessToken = await _userManager.GetAuthenticationTokenAsync(user, "Strava", "access_token");

        if (string.IsNullOrEmpty(accessToken))
        {
            _context.SystemLogs.Add(new SystemLog
            {
                Nivel = "Warning",
                Mensaje = "Token vacío",
                Detalles = $"El usuario {user.Email} intentó cargar la página sin un token válido."
            });
            await _context.SaveChangesAsync();
        }
        else
        {
            try
            {
                // 1. Descargamos las actividades personales exactas del usuario que acaba de entrar
                var apiActivities = await _stravaService.GetAthleteActivitiesAsync(accessToken);

                // Extraemos tu nombre real de la base de datos de Identity
                string nombreVisual = $"{user.Nombre} {user.Apellido}".Trim();

                // Si por alguna razón está vacío, usamos el email como respaldo
                if (string.IsNullOrEmpty(nombreVisual))
                {
                    nombreVisual = user.Email ?? "Corredor Desconocido";
                }

                // 2. Guardamos las nuevas actividades en la base de datos central
                foreach (var act in apiActivities)
                {
                    if (!await _context.Activities.AnyAsync(a => a.Id == act.Id))
                    {
                        act.UserId = user.Id;
                        act.AthleteName = nombreVisual;
                        _context.Activities.Add(act);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _context.SystemLogs.Add(new SystemLog
                {
                    Nivel = "Error",
                    Mensaje = $"Fallo API Strava - Usuario: {user.Email}",
                    Detalles = $"Atleta ID Local: {user.Id} | Token usado: {(accessToken != null && accessToken.Length > 10 ? accessToken.Substring(0, 10) + "..." : "Nulo/Corto")} | Error original: {ex.Message}"
                });
                await _context.SaveChangesAsync();
            }
        }

        // 3. CARGA DE DATOS LOCALES GLOBALES
        // Traemos TODAS las actividades de la base de datos para ver el esfuerzo de todo el equipo
        DetailedActivities = await _context.Activities
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();

        // 4. CONSOLIDACIÓN MENSUAL DEL EQUIPO
        ConsolidatedData = DetailedActivities
            .GroupBy(a => new
            {
                a.UserId,
                a.AthleteName,
                Year = a.StartDate.Year,
                Month = a.StartDate.Month
            })
            .Select(g => new ConsolidatedSummary
            {
                AthleteName = g.Key.AthleteName,
                MonthYear = $"{g.Key.Year}-{g.Key.Month:D2}",
                TotalActivities = g.Count(),
                TotalDistanceKm = Math.Round(g.Sum(a => a.Distance) / 1000, 2),
                TotalTimeMin = Math.Round(g.Sum(a => a.MovingTime) / 60.0, 2)
            })
            .OrderByDescending(s => s.TotalDistanceKm)
            .ThenBy(s => s.AthleteName)
            .ToList();
    }
}

public class ConsolidatedSummary
{
    public string AthleteName { get; set; } = string.Empty;
    public string MonthYear { get; set; } = string.Empty;
    public int TotalActivities { get; set; }
    public double TotalDistanceKm { get; set; }
    public double TotalTimeMin { get; set; }
}