using StravaTeamApp.Models;
using StravaTeamApp.Services;

namespace StravaTeamApp.Middleware;

public sealed class SystemExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public SystemExceptionLoggingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISystemLogService systemLogService)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await systemLogService.ErrorAsync(
                category: SystemLogCategories.System,
                eventName: "UnhandledRequestException",
                message:
                    "Ocurrió un error no controlado al procesar una solicitud.",
                exception: exception,
                details:
                    $"Method={context.Request.Method}",
                cancellationToken: CancellationToken.None);

            throw;
        }
    }
}
