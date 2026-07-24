namespace StravaTeamApp.Models;

public static class SystemLogCategories
{
    public const string System = "System";
    public const string Authentication = "Authentication";
    public const string Authorization = "Authorization";
    public const string Strava = "Strava";
    public const string Activities = "Activities";
    public const string Badges = "Badges";
    public const string Administration = "Administration";

    public static IReadOnlyList<string> All { get; } =
    [
        System,
        Authentication,
        Authorization,
        Strava,
        Activities,
        Badges,
        Administration
    ];
}
