using Microsoft.AspNetCore.Identity;

namespace StravaTeamApp.Models;

public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public long? StravaAthleteId { get; set; } 
}