using Serilog.Context;

namespace CulinaryBlog.API.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(
                HeaderName,
                out var requestedCorrelationId)
            && !string.IsNullOrWhiteSpace(requestedCorrelationId)
                ? requestedCorrelationId.ToString()
                : Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;

        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? context.User.FindFirst("sub")?.Value
            : null;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context);
        }
    }
}