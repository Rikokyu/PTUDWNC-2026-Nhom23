using Serilog.Context;

namespace CulinaryBlog.API.Middleware;

public sealed class CorrelationIdMiddleware
{
	public const string HeaderName = "X-Correlation-ID";
	public const string ContextItemKey = "CorrelationId";
	private readonly RequestDelegate _next;

	public CorrelationIdMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var suppliedId = context.Request.Headers[HeaderName].FirstOrDefault();
		var correlationId = IsSafeCorrelationId(suppliedId)
			? suppliedId!
			: Guid.NewGuid().ToString("N");

		context.Items[ContextItemKey] = correlationId;
		context.Response.Headers[HeaderName] = correlationId;

		using (LogContext.PushProperty("CorrelationId", correlationId))
		{
			await _next(context);
		}
	}

	public static string GetCorrelationId(HttpContext context) =>
		context.Items.TryGetValue(ContextItemKey, out var value)
			? value?.ToString() ?? string.Empty
			: string.Empty;

	private static bool IsSafeCorrelationId(string? value) =>
		!string.IsNullOrWhiteSpace(value)
		&& value.Length <= 128
		&& value.All(character => character is >= (char)0x21 and <= (char)0x7E);
}
