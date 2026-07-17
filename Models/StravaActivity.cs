namespace StravaTeamApp.Models;

public class StravaActivity
{
    public long Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public double Distance { get; set; }
    public double TotalElevationGain { get; set; }
    public int MovingTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    
    // Vínculo directo con tu usuario del Hub
    public string UserId { get; set; } = string.Empty;
    
    // Nombre guardado directamente para no hacer consultas lentas
    public string AthleteName { get; set; } = string.Empty; 
}