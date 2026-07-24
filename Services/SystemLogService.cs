using System.Security.Claims;
using Microsoft.Extensions.Logging;
using StravaTeamApp.Data;
using StravaTeamApp.Models;

namespace StravaTeamApp.Services;

public sealed class SystemLogService : ISystemLogService
{
    private const int MaxCategoryLength = 50;
    private const int MaxEventNameLength = 100;
    private const int MaxMessageLength = 500;
    private const int MaxDetailsLength = 4000;
    private const int MaxRequestIdLength = 100;
    private const int MaxRequestPathLength = 300;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SystemLogService> _logger;

    public SystemLogService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SystemLogService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task InformationAsync(
        string category,
        string eventName,
        string message,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            LogLevel.Information,
            SystemLogLevels.Information,
            category,
            eventName,
            message,
            details,
            exception: null,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);
    }

    public Task WarningAsync(
        string category,
        string eventName,
        string message,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            LogLevel.Warning,
            SystemLogLevels.Warning,
            category,
            eventName,
            message,
            details,
            exception: null,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);
    }

    public Task ErrorAsync(
        string category,
        string eventName,
        string message,
        Exception exception,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return WriteAsync(
            LogLevel.Error,
            SystemLogLevels.Error,
            category,
            eventName,
            message,
            details,
            exception: exception,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);
    }

    public Task CriticalAsync(
        string category,
        string eventName,
        string message,
        Exception exception,
        string? details = null,
        string? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return WriteAsync(
            LogLevel.Critical,
            SystemLogLevels.Critical,
            category,
            eventName,
            message,
            details,
            exception: exception,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);
    }

    private async Task WriteAsync(
        LogLevel logLevel,
        string storedLevel,
        string category,
        string eventName,
        string message,
        string? details,
        Exception? exception,
        string? actorUserId,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        actorUserId ??=
            httpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var normalizedCategory =
            NormalizeRequired(
                category,
                MaxCategoryLength,
                SystemLogCategories.System);

        var normalizedEventName =
            NormalizeRequired(
                eventName,
                MaxEventNameLength,
                "UnknownEvent");

        var normalizedMessage =
            NormalizeRequired(
                message,
                MaxMessageLength,
                "Evento sin descripción.");

        var normalizedDetails =
            NormalizeOptional(
                details,
                MaxDetailsLength);

        var exceptionType = exception?
            .GetType()
            .FullName;

        var storedDetails = string.Join(
            " | ",
            new[]
            {
                normalizedDetails,
                exceptionType is null
                    ? null
                    : $"ExceptionType={exceptionType}"
            }
            .Where(value =>
                !string.IsNullOrWhiteSpace(value)));

        _logger.Log(
            logLevel,
            exception,
            "{Category}.{EventName}: {Message} " +
            "ActorUserId={ActorUserId} RequestId={RequestId} " +
            "Details={Details}",
            normalizedCategory,
            normalizedEventName,
            normalizedMessage,
            actorUserId,
            httpContext?.TraceIdentifier,
            normalizedDetails);

        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            context.SystemLogs.Add(new SystemLog
            {
                CreatedAtUtc = DateTime.UtcNow,
                Level = storedLevel,
                Category = normalizedCategory,
                EventName = normalizedEventName,
                Message = normalizedMessage,
                Details = Truncate(
                    storedDetails,
                    MaxDetailsLength),
                ActorUserId = NormalizeOptional(
                    actorUserId,
                    450),
                RequestId = NormalizeOptional(
                    httpContext?.TraceIdentifier,
                    MaxRequestIdLength),
                RequestPath = NormalizeOptional(
                    httpContext?.Request.Path.Value,
                    MaxRequestPathLength)
            });

            await context.SaveChangesAsync(
                cancellationToken);
        }
        catch (Exception persistenceException)
        {
            _logger.LogError(
                persistenceException,
                "Could not persist system log " +
                "{Category}.{EventName}.",
                normalizedCategory,
                normalizedEventName);
        }
    }

    private static string NormalizeRequired(
        string? value,
        int maxLength,
        string fallback)
    {
        var normalized =
            NormalizeOptional(value, maxLength);

        return string.IsNullOrWhiteSpace(normalized)
            ? fallback
            : normalized;
    }

    private static string NormalizeOptional(
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();

        return Truncate(normalized, maxLength);
    }

    private static string Truncate(
        string value,
        int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
