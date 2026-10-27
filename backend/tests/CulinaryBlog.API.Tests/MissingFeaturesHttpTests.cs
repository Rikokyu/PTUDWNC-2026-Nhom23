using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Asp.Versioning;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;

namespace CulinaryBlog.API.Tests;
public sealed class PostgreSqlHttpFactAttribute : FactAttribute
{
    public PostgreSqlHttpFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CULINARYBLOG_TEST_POSTGRES")))
            Skip = "Requires CULINARYBLOG_TEST_POSTGRES; uses an isolated database.";
    }
}

public sealed class PostgreSqlHttpTheoryAttribute : TheoryAttribute
{
    public PostgreSqlHttpTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CULINARYBLOG_TEST_POSTGRES")))
            Skip = "Requires CULINARYBLOG_TEST_POSTGRES; uses an isolated database.";
    }
}

// Actual Kestrel HTTP and PostgreSQL. Only email delivery is captured by this fixture.
public sealed class MissingFeaturesHttpTests : IAsyncLifetime
{
    private readonly string database = $"culinary_http_test_{Guid.NewGuid():N}";
    private NpgsqlConnection? admin;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private readonly ResetMail mail = new();
    private bool created;
    private sealed class ResetMail : IPasswordResetEmailSender
    {
        public string? Link;
        public Task SendAsync(string email, string resetLink, CancellationToken ct) { Link = resetLink; return Task.CompletedTask; }
    }
    public async Task InitializeAsync()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("CULINARYBLOG_TEST_POSTGRES")) { Database = "postgres", Pooling = false };
        admin = new(settings.ConnectionString); await admin.OpenAsync();
        await new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin).ExecuteNonQueryAsync(); created = true;
        settings.Database = database;
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = settings.ConnectionString,
            ["Jwt:Secret"] = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            ["Hangfire:Enabled"] = "false", ["FileStorage:Provider"] = "Local",
            ["PasswordReset:ApplicationUrl"] = "https://culinary.test"
        });
        builder.Services.AddApplicationServices().AddJwtAuthentication(builder.Configuration).AddApiRateLimiting();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.Replace(ServiceDescriptor.Singleton<IPasswordResetEmailSender>(mail));
        builder.Services.AddProblemDetails(); builder.Services.AddOutputCache();
        builder.Services.AddApiVersioning(options => { options.DefaultApiVersion = new ApiVersion(1, 0); options.AssumeDefaultVersionWhenUnspecified = true; });
        app = builder.Build();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization(); app.UseOutputCache();
        app.MapAuthEndpoints();
        app.MapCategoryEndpoints();
        var version = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build();
        app.MapRecipeEndpoints(version);
        using (var scope = app.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Database.MigrateAsync();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        client = new HttpClient { BaseAddress = new Uri(address) };
    }
    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
        if (admin is not null)
        {
            // Keep isolated databases when the caller requires non-destructive verification.
            if (created && Environment.GetEnvironmentVariable("CULINARYBLOG_KEEP_TEST_DATABASES") != "1") await new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin).ExecuteNonQueryAsync();
            await admin.DisposeAsync();
        }
    }
    private async Task<string> Register()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { displayName = "HTTP Test", email = "http@example.com", password = "Original123!" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = data.GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    [PostgreSqlHttpTheory]
    [InlineData("", 200, 1, 12)]
    [InlineData("?page=2", 200, 2, 12)]
    [InlineData("?pageSize=5", 200, 1, 5)]
    [InlineData("?page=2&pageSize=5", 200, 2, 5)]
    [InlineData("?page=0", 400, 0, 0)]
    [InlineData("?pageSize=0", 400, 0, 0)]
    [InlineData("?pageSize=51", 400, 0, 0)]
    [InlineData("?page=-1", 400, 0, 0)]
    [InlineData("?pageSize=-1", 400, 0, 0)]
    [InlineData("?page=abc", 400, 0, 0)]
    [InlineData("?pageSize=abc", 400, 0, 0)]
    [InlineData("?page=", 400, 0, 0)]
    [InlineData("?page=1&page=2", 400, 0, 0)]
    [InlineData("?page=2147483647&pageSize=50", 400, 0, 0)]
    public async Task CategoryDetail_PaginationDefaultsAndValidation(string query, int status, int page, int pageSize)
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var category = CulinaryBlog.Domain.Entities.Category.Create("Pagination", "pagination", null, null, 0);
            db.Categories.Add(category);
            for (var i = 0; i < 15; i++)
                db.Recipes.Add(new CulinaryBlog.Domain.Entities.Recipe
                {
                    Id = Guid.NewGuid(), Title = $"Recipe {i}", Slug = $"recipe-{i}",
                    CategoryId = category.Id, Status = CulinaryBlog.Domain.Enums.RecipeStatus.Published,
                    Difficulty = CulinaryBlog.Domain.Enums.DifficultyLevel.Easy, CreatedAt = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }
        var response = await client.GetAsync("/api/v1/categories/pagination" + query);
        Assert.Equal(status, (int)response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (status == 200)
        {
            var recipes = body.GetProperty("recipes");
            Assert.Equal(page, recipes.GetProperty("page").GetInt32());
            Assert.Equal(pageSize, recipes.GetProperty("pageSize").GetInt32());
            Assert.Equal(15, recipes.GetProperty("totalCount").GetInt32());
            Assert.Equal(Math.Min(pageSize, 15 - (page - 1) * pageSize), recipes.GetProperty("items").GetArrayLength());
        }
        else Assert.Equal(400, body.GetProperty("status").GetInt32());
    }

    [PostgreSqlHttpFact]
    public async Task CategoryList_RemainsAnUnpaginatedArray()
    {
        foreach (var query in new[] { "", "?page=1&pageSize=12" })
        {
            var response = await client.GetAsync("/api/v1/categories" + query);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(JsonValueKind.Array, (await response.Content.ReadFromJsonAsync<JsonElement>()).ValueKind);
        }
    }
    [PostgreSqlHttpFact]
    public async Task AuthenticatedProfileAndPassword_InvalidateOldAccessToken()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PatchAsJsonAsync("/api/v1/auth/me", new { displayName = "attacker" })).StatusCode);
        await Register();
        var profile = await client.PatchAsJsonAsync("/api/v1/auth/me", new { displayName = "Updated", bio = "HTTP bio", avatarUrl = "https://example.com/a.png", role = "Admin" });
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        var data = await profile.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Author", data.GetProperty("role").GetString());
        Assert.Equal("Updated", data.GetProperty("displayName").GetString());
        using var image = new MultipartFormDataContent();
        var png = new ByteArrayContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aA1sAAAAASUVORK5CYII="));
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        image.Add(png, "file", "avatar.png");
        var uploaded = await client.PostAsync("/api/v1/auth/me/avatar", image);
        Assert.Equal(HttpStatusCode.OK, uploaded.StatusCode);
        var avatar = await uploaded.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("/uploads/avatars/", avatar.GetProperty("avatarUrl").GetString());
        var changed = await client.PostAsJsonAsync("/api/v1/auth/change-password", new { currentPassword = "Original123!", newPassword = "Changed123!", confirmPassword = "Changed123!" });
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/auth/me")).StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "http@example.com", password = "Original123!" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "http@example.com", password = "Changed123!" })).StatusCode);
    }
    [PostgreSqlHttpFact]
    public async Task Reset_ReturnsGenericResponse_ConsumesToken_AndEnforcesRateLimit()
    {
        await Register(); client.DefaultRequestHeaders.Authorization = null;
        var existing = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "http@example.com" });
        var missing = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "missing@example.com" });
        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        Assert.Equal(await existing.Content.ReadAsStringAsync(), await missing.Content.ReadAsStringAsync());
        var token = Uri.UnescapeDataString(mail.Link!.Split("&token=")[1]);
        var body = new { email = "http@example.com", token, newPassword = "Changed123!", confirmPassword = "Changed123!" };
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/auth/reset-password", body)).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await client.PostAsJsonAsync("/api/v1/auth/reset-password", body)).StatusCode);
        await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "missing@example.com" });
        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = "missing@example.com" })).StatusCode);
    }
    [PostgreSqlHttpFact]
    public async Task SearchAndList_ValidateFilters_AndSupportDescendingTitle()
    {
        foreach (var path in new[] { "/api/v1/recipes", "/api/v1/recipes/search?q=pho" })
        {
            var separator = path.Contains('?') ? "&" : "?";
            foreach (var invalid in new[] { "difficulty=999", "categoryId=bad", "maxCookTime=-1", "sort=bad", "page=2147483647&pageSize=50" })
                Assert.Equal(HttpStatusCode.UnprocessableEntity, (await client.GetAsync(path + separator + invalid)).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path + separator + "sort=-title&difficulty=Easy&maxCookTime=30")).StatusCode);
        }
    }
}
