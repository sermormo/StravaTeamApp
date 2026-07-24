namespace StravaTeamApp.Services;

public interface ISystemLogService
{
    Task InformationAsync(
        string category,
        string eventName,
        string message,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default);

    Task WarningAsync(
        string category,
        string eventName,
        string message,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default);

    Task ErrorAsync(
        string category,
        string eventName,
        string message,
        Exception exception,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default);

    Task CriticalAsync(
        string category,
        string eventName,
        string message,
        Exception exception,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default);
}
