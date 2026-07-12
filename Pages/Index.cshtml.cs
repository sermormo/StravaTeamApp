using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StravaTeamApp.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        // La página de inicio ahora es limpia y estática.
        // Toda la lógica de datos se trasladó a RunningModel.
    }
}