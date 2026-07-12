using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StravaTeamApp.Models;

namespace StravaTeamApp.Services;

public class StravaAthleteService
{
    private readonly HttpClient _httpClient;

    public StravaAthleteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.strava.com/api/v3/");
    }

    public async Task<List<StravaActivity>> GetAthleteActivitiesAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync("athlete/activities?per_page=50");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var apiData = JsonSerializer.Deserialize<List<StravaActivityDto>>(content, options);
        
        return apiData?.Where(a => a.Type == "Run").Select(a => new StravaActivity
        {
            Id = a.Id, 
            Name = a.Name,
            Distance = a.Distance,
            MovingTime = a.MovingTime,
            StartDate = a.StartDate
        }).ToList() ?? new List<StravaActivity>();
    }

    public async Task<bool> EsMiembroDelClubAsync(string accessToken, long clubId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync("athlete/clubs");
        if (!response.IsSuccessStatusCode) return false;

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var clubes = JsonSerializer.Deserialize<List<ClubSummaryDto>>(content, options);
        
        return clubes?.Any(c => c.Id == clubId) ?? false;
    }

    private class StravaActivityDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        [JsonPropertyName("moving_time")]
        public int MovingTime { get; set; }
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    private class ClubSummaryDto { public long Id { get; set; } }
}