using Serilog.Context;
using System.Diagnostics;

namespace CulinaryBlog.API.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var requested = context.Request.Headers.TryGetValue(
                HeaderName,
                out var requestedCorrelationId)
            && !string.IsNullOrWhiteSpace(requestedCorrelationId)
                ? requestedCorrelationId.ToString()
                : null;
        var correlationId = requested is { Length: > 0 and <= 64 }
            && requested.All(character =>
                char.IsAsciiLetterOrDigit(character) || character is '-' or '_')
                ? requested
                : Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
        {
            await next(context);
        }
    }
}
