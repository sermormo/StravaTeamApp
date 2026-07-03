using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StravaTeamApp.Models;

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
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.GetAsync($"clubs/{clubId}/activities?page=1&per_page=100");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var apiData = JsonSerializer.Deserialize<List<StravaActivityDto>>(content, options);
        var finalActivities = new List<StravaActivity>();

        if (apiData != null)
        {
            var currentClub = new Club { Id = clubId, Name = "Equipo Principal" };

            foreach (var item in apiData.Where(a => a.Type == "Run"))
            {
                // 1. Usamos nuestra nueva función estable para el ID del atleta
                string fullName = (item.Athlete?.Firstname ?? "") + (item.Athlete?.Lastname ?? "");
                long tempAthleteId = GenerateStableId(fullName);

                // 2. Usamos la función estable para el ID de la carrera
                string uniqueHash = $"{item.Athlete?.Firstname}_{item.Name}_{item.Distance}_{item.MovingTime}";
                long generatedActivityId = GenerateStableId(uniqueHash);

                finalActivities.Add(new StravaActivity
                {
                    Id = generatedActivityId,
                    Name = item.Name,
                    Distance = item.Distance,
                    MovingTime = item.MovingTime,
                    Type = item.Type,
                    StartDate = DateTime.Now,
                    AthleteId = tempAthleteId,
                    Athlete = new Athlete
                    {
                        Id = tempAthleteId,
                        Firstname = item.Athlete?.Firstname ?? "Atleta",
                        Lastname = item.Athlete?.Lastname ?? "",
                        ClubId = clubId,
                        Club = currentClub
                    }
                });
            }
        }
        return finalActivities;
    }


    public async Task<bool> ValidarAtletaEnClubAsync(long clubId, string userAccessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken);

        // Consultamos el endpoint personal del atleta
        var response = await _httpClient.GetAsync("athlete/clubs");
        if (!response.IsSuccessStatusCode) return false;

        var content = await response.Content.ReadAsStringAsync();
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Leemos la lista de clubes a los que pertenece
        var clubesDelAtleta = System.Text.Json.JsonSerializer.Deserialize<List<ClubSummaryDto>>(content, options);

        if (clubesDelAtleta == null) return false;

        // Verificamos si el ID de nuestro equipo está en su lista personal
        return clubesDelAtleta.Any(c => c.Id == clubId);
    }


    // --- NUEVA FUNCIÓN MATEMÁTICA ESTABLE ---
    // Siempre devuelve el mismo ID numérico para el mismo texto
    private long GenerateStableId(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        long hash = 5381;
        foreach (char c in text)
        {
            hash = ((hash << 5) + hash) + c;
        }
        return Math.Abs(hash);
    }

    private class StravaActivityDto
    {
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }

        [JsonPropertyName("moving_time")]
        public int MovingTime { get; set; }

        public string Type { get; set; } = string.Empty;
        public AthleteSummaryDto? Athlete { get; set; }
    }

    private class AthleteSummaryDto
    {
        public long Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
    }

    private class ClubSummaryDto
    {
        public long Id { get; set; }
    }

}