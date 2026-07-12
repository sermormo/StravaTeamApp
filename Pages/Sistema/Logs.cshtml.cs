using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StravaTeamApp.Data;
using StravaTeamApp.Models;

namespace StravaTeamApp.Pages.Sistema;

// Protegemos la página para que solo usuarios autenticados puedan verla. 
// A futuro, aquí podrías aplicar [Authorize(Roles = "Admin")]
[Authorize]
public class LogsModel : PageModel
{
    private readonly AppDbContext _context;

    public List<SystemLog> Registros { get; set; } = new();

    public LogsModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        // Traemos los últimos 100 registros ordenados por fecha descendente
        Registros = await _context.SystemLogs
            .OrderByDescending(l => l.Fecha)
            .Take(100)
            .ToListAsync();
    }
    public async Task<IActionResult> OnPostBorrarTodosAsync()
    {
        var todosLosLogs = await _context.SystemLogs.ToListAsync();
        _context.SystemLogs.RemoveRange(todosLosLogs);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

}