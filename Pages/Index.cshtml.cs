using Microsoft.AspNetCore.Mvc.RazorPages;
using StravaTeamApp.Services;

namespace StravaTeamApp.Pages;

public class IndexModel : PageModel
{
    private readonly StravaClubService _stravaService;
    
    public List<StravaActivity> Activities { get; set; } = new();

    public IndexModel(StravaClubService stravaService)
    {
        _stravaService = stravaService;
    }

    public async Task OnGetAsync()
    {
        long clubId = 1252154; 
        string accessToken = "fcf78af20c52c5cfe522f9769ed924ccb7dd06be"; 

        try 
        {
            Activities = await _stravaService.GetClubRunsAsync(clubId, accessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo datos: {ex.Message}");
        }
    }
}