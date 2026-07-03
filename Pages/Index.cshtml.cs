using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StravaTeamApp.Services;
using StravaTeamApp.Data;
using StravaTeamApp.Models;

namespace StravaTeamApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly StravaClubService _stravaService;
    private readonly AppDbContext _context;
    
    // Lista para el bloque consolidado por mes
    public List<ConsolidatedSummary> ConsolidatedData { get; set; } = new();
    
    // Lista para mantener el historial completo abajo si es necesario
    public List<StravaActivity> DetailedActivities { get; set; } = new();

    public IndexModel(StravaClubService stravaService, AppDbContext context)
    {
        _stravaService = stravaService;
        _context = context;
    }

    public async Task OnGetAsync()
    {
        long clubId = 1252154; // Tu ID de Club real
        string accessToken = "8fc747513a2fa3f469f872518f8dedb2dd2731de"; // Tu Token actual

        try 
        {
            var club = await _context.Clubs.FindAsync(clubId);
            if (club == null)
            {
                _context.Clubs.Add(new Club { Id = clubId, Name = "Equipo Principal" });
                await _context.SaveChangesAsync();
            }

            var rawApiActivities = await _stravaService.GetClubRunsAsync(clubId, accessToken);
            var apiActivities = rawApiActivities.DistinctBy(a => a.Id).ToList();

            var uniqueAthletes = apiActivities
                .Where(a => a.Athlete != null)
                .Select(a => a.Athlete!)
                .DistinctBy(a => a.Id)
                .ToList();

            foreach (var athlete in uniqueAthletes)
            {
                if (!await _context.Athletes.AnyAsync(a => a.Id == athlete.Id))
                {
                    athlete.Club = null;
                    _context.Athletes.Add(athlete);
                }
            }

            foreach (var act in apiActivities)
            {
                if (!await _context.Activities.AnyAsync(a => a.Id == act.Id))
                {
                    act.Athlete = null;
                    _context.Activities.Add(act);
                }
            }
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error de sincronización: {ex.Message}");
        }
        finally
        {
            // 1. Cargar todas las actividades de la base de datos con sus atletas
            DetailedActivities = await _context.Activities
                .Include(a => a.Athlete)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

           // 2. CONSOLIDACIÓN LÓGICA: Agrupar por Atleta y por Mes/Año
            ConsolidatedData = DetailedActivities
                .Where(a => a.Athlete != null)
                .GroupBy(a => new 
                { 
                    a.AthleteId, 
                    a.Athlete!.Firstname, 
                    a.Athlete!.Lastname,
                    Year = a.StartDate.Year, 
                    Month = a.StartDate.Month 
                })
                .Select(g => new ConsolidatedSummary
                {
                    AthleteName = $"{g.Key.Firstname} {g.Key.Lastname}",
                    MonthYear = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalActivities = g.Count(),
                    TotalDistanceKm = Math.Round(g.Sum(a => a.Distance) / 1000, 2),
                    TotalTimeMin = Math.Round(g.Sum(a => a.MovingTime) / 60.0, 2)
                })
                // ¡AQUÍ ESTÁ EL CAMBIO! Ordenamos por distancia de mayor a menor por defecto
                .OrderByDescending(s => s.TotalDistanceKm)
                .ThenBy(s => s.AthleteName)
                .ToList();
        }
    }
}

// Estructura para almacenar los datos consolidados
public class ConsolidatedSummary
{
    public string AthleteName { get; set; } = string.Empty;
    public string MonthYear { get; set; } = string.Empty;
    public int TotalActivities { get; set; }
    public double TotalDistanceKm { get; set; }
    public double TotalTimeMin { get; set; }
}