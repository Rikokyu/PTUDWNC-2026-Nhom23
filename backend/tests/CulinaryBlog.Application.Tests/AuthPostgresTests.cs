using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class AuthPostgresTests
{
    private static readonly string? Connection = Environment.GetEnvironmentVariable("CULINARY_AUTH_TEST_DB");

    private static CulinaryBlogDbContext Db() => new(
        new DbContextOptionsBuilder<CulinaryBlogDbContext>().UseNpgsql(Connection).Options);

    private static IdentityService Service(CulinaryBlogDbContext db, IGoogleTokenVerifier? google = null)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "auth-isolated-test-secret-with-at-least-thirty-two-bytes",
            ["Jwt:Issuer"] = "AuthTest", ["Jwt:Audience"] = "AuthTest"
        }).Build();
        var provider = new ServiceCollection().AddDataProtection().Services.BuildServiceProvider();
        return new IdentityService(db, new PasswordHasher<ApplicationUser>(), config,
            new NoEmail(), provider.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>(),
            google ?? new FakeGoogle(null), NullLogger<IdentityService>.Instance);
    }

    [AuthPostgresFact]
    public async Task Rotation_Concurrency_Reuse_Expiry_Active_And_Ownership()
    {
        await using (var db = Db()) await db.Database.MigrateAsync();
        var email = $"auth-{Guid.NewGuid():N}@example.test";
        string first;
        Guid owner;
        await using (var db = Db())
        {
            var session = await Service(db).RegisterAsync("Auth User", email, "SecurePass1!", default);
            first = session.RefreshToken; owner = session.User.Id;
        }
        async Task<string?> Rotate(string token)
        {
            await using var db = Db();
            return (await Service(db).RefreshAsync(token, default))?.RefreshToken;
        }
        var results = await Task.WhenAll(Rotate(first), Rotate(first));
        Assert.Single(results.Where(result => result is not null));
        var next = Assert.Single(results.Where(result => result is not null))!;
        Assert.Null(await Rotate(next)); // Reuse of first revoked the entire family.

        string expired;
        await using (var db = Db())
        {
            expired = (await Service(db).LoginAsync(email, "SecurePass1!", default))!.RefreshToken;
            var hash = Hash(expired);
            await db.RefreshTokens.Where(token => token.TokenHash == hash)
                .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.ExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
        }
        Assert.Null(await Rotate(expired));

        string owned;
        await using (var db = Db())
            owned = (await Service(db).LoginAsync(email, "SecurePass1!", default))!.RefreshToken;
        await using (var db = Db()) await Service(db).LogoutAsync(Guid.NewGuid(), owned, default);
        Assert.NotNull(await Rotate(owned));

        string logout;
        await using (var db = Db())
            logout = (await Service(db).LoginAsync(email, "SecurePass1!", default))!.RefreshToken;
        await using (var db = Db())
        {
            await Service(db).LogoutAsync(owner, logout, default);
            await Service(db).LogoutAsync(owner, logout, default);
        }
        Assert.Null(await Rotate(logout));

        string inactive;
        await using (var db = Db())
            inactive = (await Service(db).LoginAsync(email, "SecurePass1!", default))!.RefreshToken;
        await using (var db = Db())
            await db.ApplicationUsers.Where(user => user.Id == owner)
                .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.IsActive, false));
        Assert.Null(await Rotate(inactive));
    }

    [AuthPostgresFact]
    public async Task Lockout_Patch_And_Google_Subject()
    {
        await using (var db = Db()) await db.Database.MigrateAsync();
        var email = $"auth-{Guid.NewGuid():N}@example.test";
        Guid id;
        await using (var db = Db()) id = (await Service(db).RegisterAsync("Original Name", email, "SecurePass1!", default)).User.Id;
        await using (var db = Db())
        {
            var updated = await Service(db).PatchProfileAsync(id, false, null, false, null, true, "New bio", default);
            Assert.Equal("Original Name", updated.DisplayName);
            Assert.Equal("New bio", updated.Bio);
            await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.ValidationException>(() =>
                Service(db).PatchProfileAsync(id, false, null, true, "javascript:bad", false, null, default));
        }
        for (var i = 0; i < 4; i++)
        {
            await using var db = Db();
            Assert.Null(await Service(db).LoginAsync(email, "wrong", default));
        }
        await using (var db = Db())
            await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.AccountLockedException>(() =>
                Service(db).LoginAsync(email, "wrong", default));
        await using (var db = Db())
            await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.AccountLockedException>(() =>
                Service(db).LoginAsync(email, "SecurePass1!", default));
        await using (var db = Db())
        {
            await db.ApplicationUsers.Where(user => user.Id == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(user => user.LockoutEnd, DateTime.UtcNow.AddMinutes(-1)));
            Assert.NotNull(await Service(db).LoginAsync(email, "SecurePass1!", default));
        }
        var google = new FakeGoogle(new VerifiedGoogleUser("subject-" + Guid.NewGuid(), email, "Google User", null));
        await using (var db = Db()) Assert.Equal(id, (await Service(db, google).GoogleLoginAsync("test", default)).User.Id);
        await using (var db = Db()) Assert.Equal(id, (await Service(db, google).GoogleLoginAsync("test", default)).User.Id);
        await using (var db = Db())
            await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.UnauthorizedException>(() =>
                Service(db, new FakeGoogle(null)).GoogleLoginAsync("bad", default));
    }

    private static string Hash(string token) => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    private sealed class NoEmail : IEmailSender
    {
        public bool IsConfigured => false;
        public Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
    private sealed class FakeGoogle(VerifiedGoogleUser? user) : IGoogleTokenVerifier
    {
        public Task<VerifiedGoogleUser?> VerifyAsync(string idToken, CancellationToken cancellationToken) =>
            Task.FromResult(user);
    }
}

public sealed class AuthPostgresFactAttribute : FactAttribute
{
    public AuthPostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CULINARY_AUTH_TEST_DB")))
            Skip = "Requires an explicitly configured disposable PostgreSQL database.";
    }
}
