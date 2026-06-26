using System.Net.Http.Headers;
using System.Text.Json;

namespace StravaTeamApp.Services;

public class StravaClubService
{
    private readonly HttpClient _httpClient;

    public StravaClubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.strava.com/api/v3/");
    }

    public async Task<List<StravaActivity>> GetClubRunsAsync(long clubId, string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync($"clubs/{clubId}/activities");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var activities = JsonSerializer.Deserialize<List<StravaActivity>>(content, options);

        return activities?.Where(a => a.Type == "Run").ToList() ?? new List<StravaActivity>();
    }
}

public class StravaActivity
{
    public string Name { get; set; } = string.Empty;
    public double Distance { get; set; } 
    public int MovingTime { get; set; } 
    public string Type { get; set; } = string.Empty;
    public AthleteSummary? Athlete { get; set; }
}

public class AthleteSummary
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}