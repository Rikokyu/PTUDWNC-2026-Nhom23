using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/users", async (HttpRequest request, IUserDirectory directory, CancellationToken ct) =>
        {
            if (!TryPage(request, "page", 1, out var page) || !TryPage(request, "pageSize", 12, out var size)
                || page < 1 || size is < 1 or > 50 || (long)(page - 1) * size > int.MaxValue)
                return Results.Problem(statusCode: 400, detail: "Invalid pagination.");
            var search = request.Query["search"].FirstOrDefault();
            var role = request.Query["role"].FirstOrDefault();
            if (search?.Length > 100 || (role is not null && role is not ("Author" or "Admin")))
                return Results.Problem(statusCode: 400, detail: "Invalid directory filters.");
            return Results.Ok(await directory.GetAsync(page, size, search, role, ct));
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Author")).WithTags("Users");
        return endpoints;
    }

    private static bool TryPage(HttpRequest request, string key, int fallback, out int value)
    {
        value = fallback;
        return !request.Query.ContainsKey(key) || int.TryParse(request.Query[key], out value);
    }
}
