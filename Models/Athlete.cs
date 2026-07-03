namespace StravaTeamApp.Models;

public class Athlete
{
    public long Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    
    // Relación con el Club
    public long ClubId { get; set; }
    public Club? Club { get; set; }
    
    // Relación con las Actividades
    public List<StravaActivity> Activities { get; set; } = new();
}