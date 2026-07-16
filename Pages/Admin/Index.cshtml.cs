using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StravaTeamApp.Pages.Admin;

[Authorize(Roles = "Administrator")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}