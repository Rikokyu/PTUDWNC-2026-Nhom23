using CulinaryBlog.Infrastructure.Persistence;

namespace CulinaryBlog.API.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health/live", () => Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        })).WithTags("Health");

        endpoints.MapGet("/health/ready", CheckDatabase)
            .WithTags("Health");
        endpoints.MapGet("/health", CheckDatabase)
            .WithTags("Health");

        return endpoints;
    }

    private static async Task<IResult> CheckDatabase(
        CulinaryBlogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var databaseHealthy = await dbContext.Database
            .CanConnectAsync(cancellationToken);
        var response = new
        {
            status = databaseHealthy ? "Healthy" : "Unhealthy",
            timestamp = DateTime.UtcNow,
            components = new
            {
                postgresql = databaseHealthy ? "Healthy" : "Unhealthy"
            }
        };

        return databaseHealthy
            ? Results.Ok(response)
            : Results.Json(
                response,
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
