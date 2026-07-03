namespace StravaTeamApp.Models;

public class Club
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Athlete> Athletes { get; set; } = new();
}