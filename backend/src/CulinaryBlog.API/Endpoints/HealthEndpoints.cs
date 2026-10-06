using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CulinaryBlog.API.Endpoints;

public static class HealthEndpoints
{
	public static IEndpointRouteBuilder MapHealthEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet(
			"/health",
			(HealthCheckService healthCheckService,
				CancellationToken cancellationToken) =>
				GetHealthAsync(
					healthCheckService,
					tag: null,
					includeChecks: true,
					cancellationToken))
			.WithTags("Health")
			.WithName("Health")
			.Produces<HealthEndpointResponse>(StatusCodes.Status200OK)
			.Produces<HealthEndpointResponse>(
				StatusCodes.Status503ServiceUnavailable);

		endpoints.MapGet(
			"/health/live",
			(HealthCheckService healthCheckService,
				CancellationToken cancellationToken) =>
				GetHealthAsync(
					healthCheckService,
					"live",
					includeChecks: false,
					cancellationToken))
			.WithTags("Health")
			.WithName("Liveness")
			.Produces<HealthEndpointResponse>(StatusCodes.Status200OK)
			.Produces<HealthEndpointResponse>(
				StatusCodes.Status503ServiceUnavailable);

		endpoints.MapGet(
			"/health/ready",
			(HealthCheckService healthCheckService,
				CancellationToken cancellationToken) =>
				GetHealthAsync(
					healthCheckService,
					"ready",
					includeChecks: true,
					cancellationToken))
			.WithTags("Health")
			.WithName("Readiness")
			.Produces<HealthEndpointResponse>(StatusCodes.Status200OK)
			.Produces<HealthEndpointResponse>(
				StatusCodes.Status503ServiceUnavailable);

		return endpoints;
	}

	private static async Task<IResult> GetHealthAsync(
		HealthCheckService healthCheckService,
		string? tag,
		bool includeChecks,
		CancellationToken cancellationToken)
	{
		var report = await healthCheckService.CheckHealthAsync(
			tag is null
				? null
				: check => check.Tags.Contains(tag),
			cancellationToken);

		var checks = includeChecks
			? report.Entries.ToDictionary(
				entry => entry.Key,
				entry => entry.Value.Status.ToString())
			: null;

		var response = new HealthEndpointResponse(
			report.Status.ToString(),
			checks);

		var statusCode = report.Status == HealthStatus.Healthy
			? StatusCodes.Status200OK
			: StatusCodes.Status503ServiceUnavailable;

		return Results.Json(
			response,
			JsonSerializerOptions.Web,
			statusCode: statusCode);
	}
}

public sealed record HealthEndpointResponse(
	string Status,
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	IReadOnlyDictionary<string, string>? Checks);
