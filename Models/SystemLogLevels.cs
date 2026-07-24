namespace StravaTeamApp.Models;

public static class SystemLogLevels
{
    public const string Information = "Information";
    public const string Warning = "Warning";
    public const string Error = "Error";
    public const string Critical = "Critical";

    public static IReadOnlyList<string> All { get; } =
    [
        Information,
        Warning,
        Error,
        Critical
    ];
}
