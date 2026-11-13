using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class AuthHttpPostgresTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private readonly string _connection = Environment.GetEnvironmentVariable("CULINARY_AUTH_HTTP_TEST_DB")
        ?? throw new InvalidOperationException("Set CULINARY_AUTH_HTTP_TEST_DB to a disposable database.");

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _connection,
            ["Jwt:Secret"] = "auth-http-isolated-test-secret-at-least-thirty-two-bytes",
            ["Jwt:Issuer"] = "AuthHttpTest", ["Jwt:Audience"] = "AuthHttpTest",
            ["Hangfire:Enabled"] = "false"
        });
        builder.Services.AddApplicationServices();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddSingleton<IGoogleTokenVerifier>(new TestGoogleVerifier());
        builder.Services.AddApiRateLimiting();
        builder.Services.AddProblemDetails();
        _app = builder.Build();
        _app.UseMiddleware<GlobalExceptionMiddleware>();
        _app.UseRouting();
        // Auth scenarios exceed the production 10-request/minute policy.
        // Rate limiting is outside this identity suite's assertions.
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapAuthEndpoints();
        await using (var scope = _app.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>().Database.MigrateAsync();
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private async Task<(Guid Id, string Access, string Refresh)> RegisterAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            displayName = "Auth HTTP User", email = $"http-{Guid.NewGuid():N}@example.test", password = "SecurePass1!"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        return (root.GetProperty("user").GetProperty("id").GetGuid(),
            root.GetProperty("accessToken").GetString()!, root.GetProperty("refreshToken").GetString()!);
    }

    private HttpRequestMessage Auth(HttpMethod method, string path, string access, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
        if (body is not null) request.Content = JsonContent.Create(body);
        return request;
    }

    [Fact]
    public async Task Refresh_Concurrent_Reuse_Expiry_And_Logout_Ownership()
    {
        var owner = await RegisterAsync();
        async Task<HttpResponseMessage> Rotate(string token) =>
            await _client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = token });
        var pair = await Task.WhenAll(Rotate(owner.Refresh), Rotate(owner.Refresh));
        Assert.Single(pair, result => result.StatusCode == HttpStatusCode.OK);
        Assert.Single(pair, result => result.StatusCode == HttpStatusCode.Unauthorized);
        var successful = pair.Single(result => result.StatusCode == HttpStatusCode.OK);
        using var rotatedJson = JsonDocument.Parse(await successful.Content.ReadAsStringAsync());
        var rotated = rotatedJson.RootElement.GetProperty("refreshToken").GetString()!;
        Assert.Equal(HttpStatusCode.Unauthorized, (await Rotate(rotated)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Rotate(owner.Refresh)).StatusCode);

        var other = await RegisterAsync();
        var victim = await RegisterAsync();
        using (var wrongOwner = await _client.SendAsync(Auth(HttpMethod.Post, "/api/v1/auth/logout",
            other.Access, new { refreshToken = victim.Refresh })))
        {
            Assert.Equal(HttpStatusCode.NoContent, wrongOwner.StatusCode);
            Assert.Equal(0, (await wrongOwner.Content.ReadAsByteArrayAsync()).Length);
        }
        Assert.Equal(HttpStatusCode.OK, (await Rotate(victim.Refresh)).StatusCode);
        using (var logout = await _client.SendAsync(Auth(HttpMethod.Post, "/api/v1/auth/logout",
            other.Access, new { refreshToken = other.Refresh })))
        {
            Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
            Assert.Equal(0, (await logout.Content.ReadAsByteArrayAsync()).Length);
        }
        using (var repeated = await _client.SendAsync(Auth(HttpMethod.Post, "/api/v1/auth/logout",
            other.Access, new { refreshToken = other.Refresh })))
            Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Rotate(other.Refresh)).StatusCode);

        var expiring = await RegisterAsync();
        await using (var scope = _app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(expiring.Refresh)));
            await db.RefreshTokens.Where(token => token.TokenHash == hash).ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await Rotate(expiring.Refresh)).StatusCode);
    }

    [Fact]
    public async Task Login_Lockout_Inactive_And_Patch_Partial_Clear_Validation()
    {
        var account = await RegisterAsync();
        using (var patch = await _client.SendAsync(Auth(HttpMethod.Patch, "/api/v1/auth/me", account.Access,
            new { bio = "Added bio" })))
        {
            Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
            using var json = JsonDocument.Parse(await patch.Content.ReadAsStringAsync());
            Assert.Equal("Auth HTTP User", json.RootElement.GetProperty("displayName").GetString());
            Assert.Equal("Added bio", json.RootElement.GetProperty("bio").GetString());
        }
        using (var clear = await _client.SendAsync(Auth(HttpMethod.Patch, "/api/v1/auth/me", account.Access,
            new { bio = (string?)null })))
        {
            Assert.Equal(HttpStatusCode.OK, clear.StatusCode);
            using var json = JsonDocument.Parse(await clear.Content.ReadAsStringAsync());
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("bio").ValueKind);
        }
        using (var invalid = await _client.SendAsync(Auth(HttpMethod.Patch, "/api/v1/auth/me", account.Access,
            new { avatarUrl = "javascript:bad" })))
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
            using var json = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
            Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("avatarUrl", out _));
        }
        using (var forbiddenField = await _client.SendAsync(Auth(HttpMethod.Patch, "/api/v1/auth/me", account.Access,
            new { email = "changed@example.test" })))
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, forbiddenField.StatusCode);
            using var json = JsonDocument.Parse(await forbiddenField.Content.ReadAsStringAsync());
            Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("email", out _));
        }
        using (var weak = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { displayName = "Weak User", email = "weak@example.test", password = "weak" }))
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, weak.StatusCode);
            using var json = JsonDocument.Parse(await weak.Content.ReadAsStringAsync());
            Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("password", out _));
        }
        using (var noJwt = await _client.PatchAsJsonAsync("/api/v1/auth/me", new { bio = "x" }))
            Assert.Equal(HttpStatusCode.Unauthorized, noJwt.StatusCode);

        var email = $"lock-{Guid.NewGuid():N}@example.test";
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/v1/auth/register",
            new { displayName = "Lock User", email, password = "SecurePass1!" })).StatusCode);
        for (var i = 0; i < 4; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync("/api/v1/auth/login",
                new { email, password = "WrongPass1!" })).StatusCode);
        Assert.Equal((HttpStatusCode)423, (await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "WrongPass1!" })).StatusCode);
        Assert.Equal((HttpStatusCode)423, (await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email, password = "SecurePass1!" })).StatusCode);
        await using (var scope = _app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlogDbContext>();
            await db.ApplicationUsers.Where(user => user.Id == account.Id).ExecuteUpdateAsync(
                setters => setters.SetProperty(user => user.IsActive, false));
            var inactiveEmail = await db.ApplicationUsers.Where(user => user.Id == account.Id)
                .Select(user => user.Email).SingleAsync();
            Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync("/api/v1/auth/login",
                new { email = inactiveEmail, password = "SecurePass1!" })).StatusCode);
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync("/api/v1/auth/refresh",
            new { refreshToken = account.Refresh })).StatusCode);
        using var inactiveJwt = await _client.SendAsync(Auth(HttpMethod.Get, "/api/v1/auth/me", account.Access));
        Assert.Equal(HttpStatusCode.Unauthorized, inactiveJwt.StatusCode);
    }

    [Fact]
    public async Task Google_Route_Uses_Server_Verified_Profile()
    {
        using var invalid = await _client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "invalid" });
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        using var first = await _client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "test-id-token" });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        using var firstJson = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        var id = firstJson.RootElement.GetProperty("user").GetProperty("id").GetGuid();
        using var repeated = await _client.PostAsJsonAsync("/api/v1/auth/google", new { idToken = "test-id-token" });
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        using var repeatedJson = JsonDocument.Parse(await repeated.Content.ReadAsStringAsync());
        Assert.Equal(id, repeatedJson.RootElement.GetProperty("user").GetProperty("id").GetGuid());
    }

    private sealed class TestGoogleVerifier : IGoogleTokenVerifier
    {
        public Task<VerifiedGoogleUser?> VerifyAsync(string idToken, CancellationToken cancellationToken) =>
            Task.FromResult<VerifiedGoogleUser?>(idToken == "test-id-token"
                ? new("http-test-google-subject", "http-google@example.test", "Google Test", null)
                : null);
    }
}
