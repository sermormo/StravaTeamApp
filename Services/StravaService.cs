using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using StravaTeamApp.Models;

namespace StravaTeamApp.Services;

public sealed class StravaService
{
    private const string LoginProvider = "Strava";

    private static readonly TimeSpan RefreshThreshold =
        TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly HttpClient _httpClient;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public StravaService(
        HttpClient httpClient,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress =
            new Uri("https://www.strava.com/api/v3/");

        _userManager = userManager;

        _clientId =
            configuration["Strava:ClientId"]
            ?? throw new InvalidOperationException(
                "Missing configuration: Strava:ClientId");

        _clientSecret =
            configuration["Strava:ClientSecret"]
            ?? throw new InvalidOperationException(
                "Missing configuration: Strava:ClientSecret");
    }

    public async Task<string?> GetValidAccessTokenAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        var accessToken =
            await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider,
                "access_token");

        var expiresAt =
            await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider,
                "expires_at");

        if (!string.IsNullOrWhiteSpace(accessToken) &&
            HasEnoughLifetime(expiresAt))
        {
            return accessToken;
        }

        var refreshToken =
            await _userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider,
                "refresh_token");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        return await RefreshAccessTokenAsync(
            user,
            refreshToken,
            cancellationToken);
    }

    public async Task<List<StravaActivity>>
        GetAthleteActivitiesAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
    {
        using var request =
            CreateAuthorizedRequest(
                HttpMethod.Get,
                "athlete/activities?per_page=50",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var apiActivities =
            await response.Content
                .ReadFromJsonAsync<List<StravaActivityDto>>(
                    JsonOptions,
                    cancellationToken);

        return apiActivities?
            .Where(activity => activity.Type == "Run")
            .Select(activity => new StravaActivity
            {
                Id = activity.Id,
                Name = activity.Name,
                Distance = activity.Distance,
                TotalElevationGain =
                    activity.TotalElevationGain,
                MovingTime = activity.MovingTime,
                StartDate = activity.StartDate,
                Type = activity.Type
            })
            .ToList()
            ?? new List<StravaActivity>();
    }

    public async Task<bool> IsClubMemberAsync(
        string accessToken,
        long clubId,
        CancellationToken cancellationToken = default)
    {
        using var request =
            CreateAuthorizedRequest(
                HttpMethod.Get,
                "athlete/clubs",
                accessToken);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var clubs =
            await response.Content
                .ReadFromJsonAsync<List<ClubSummaryDto>>(
                    JsonOptions,
                    cancellationToken);

        return clubs?.Any(club => club.Id == clubId)
            ?? false;
    }

    private async Task<string> RefreshAccessTokenAsync(
        ApplicationUser user,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        using var content =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken
                });

        using var response =
            await _httpClient.PostAsync(
                "https://www.strava.com/api/v3/oauth/token",
                content,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse =
            await response.Content
                .ReadFromJsonAsync<StravaTokenResponse>(
                    JsonOptions,
                    cancellationToken);

        if (tokenResponse is null ||
            string.IsNullOrWhiteSpace(
                tokenResponse.AccessToken) ||
            string.IsNullOrWhiteSpace(
                tokenResponse.RefreshToken))
        {
            throw new InvalidOperationException(
                "Strava returned an invalid token response.");
        }

        // Strava can invalidate the previous refresh token
        // immediately, so the newest one is stored first.
        await SaveTokenAsync(
            user,
            "refresh_token",
            tokenResponse.RefreshToken);

        await SaveTokenAsync(
            user,
            "access_token",
            tokenResponse.AccessToken);

        var expiration =
            DateTimeOffset
                .FromUnixTimeSeconds(
                    tokenResponse.ExpiresAt)
                .ToString(
                    "O",
                    CultureInfo.InvariantCulture);

        await SaveTokenAsync(
            user,
            "expires_at",
            expiration);

        if (!string.IsNullOrWhiteSpace(
            tokenResponse.TokenType))
        {
            await SaveTokenAsync(
                user,
                "token_type",
                tokenResponse.TokenType);
        }

        return tokenResponse.AccessToken;
    }

    private async Task SaveTokenAsync(
        ApplicationUser user,
        string name,
        string value)
    {
        var result =
            await _userManager
                .SetAuthenticationTokenAsync(
                    user,
                    LoginProvider,
                    name,
                    value);

        if (result.Succeeded)
        {
            return;
        }

        var errors =
            string.Join(
                ", ",
                result.Errors.Select(
                    error => error.Description));

        throw new InvalidOperationException(
            $"Could not save Strava token '{name}': {errors}");
    }

    private static bool HasEnoughLifetime(
        string? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(expiresAt))
        {
            return false;
        }

        DateTimeOffset expiration;

        if (long.TryParse(
            expiresAt,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var unixSeconds))
        {
            expiration =
                DateTimeOffset.FromUnixTimeSeconds(
                    unixSeconds);
        }
        else if (!DateTimeOffset.TryParse(
            expiresAt,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out expiration))
        {
            return false;
        }

        return expiration >
            DateTimeOffset.UtcNow.Add(
                RefreshThreshold);
    }

    private static HttpRequestMessage
        CreateAuthorizedRequest(
            HttpMethod method,
            string requestUri,
            string accessToken)
    {
        var request =
            new HttpRequestMessage(
                method,
                requestUri);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return request;
    }

    private sealed class StravaTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } =
            string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } =
            string.Empty;

        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } =
            string.Empty;
    }

    private sealed class StravaActivityDto
    {
        public long Id { get; set; }

        public string Name { get; set; } =
            string.Empty;

        public double Distance { get; set; }

        [JsonPropertyName("total_elevation_gain")]
        public double TotalElevationGain { get; set; }

        [JsonPropertyName("moving_time")]
        public int MovingTime { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        public string Type { get; set; } =
            string.Empty;
    }

    private sealed class ClubSummaryDto
    {
        public long Id { get; set; }
    }
}