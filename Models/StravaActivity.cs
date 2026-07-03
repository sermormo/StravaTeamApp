namespace StravaTeamApp.Models;

public class StravaActivity
{
    public long Id { get; set; } // Agregamos el ID de la carrera
    public string Name { get; set; } = string.Empty;
    public double Distance { get; set; } 
    public int MovingTime { get; set; } 
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    // Relación con el Atleta
    public long AthleteId { get; set; }
    public Athlete? Athlete { get; set; }
}