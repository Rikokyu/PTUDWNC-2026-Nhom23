using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class AuthModuleTests
{
    [Fact]
    public async Task Register_RejectsBlankDisplayNameBeforeCallingAuthService()
    {
        var authService = new FakeAuthService();
        var handler = new RegisterCommandHandler(authService);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new RegisterCommand("  ", "jamie@example.com", "password123"),
                CancellationToken.None));

        Assert.False(authService.RegisterCalled);
    }

    [Fact]
    public void JwtService_GeneratesValidHs256BearerToken()
    {
        const string secret = "a-long-test-secret-with-at-least-32-bytes";
        const string issuer = "CulinaryBlog.Tests";
        const string audience = "CulinaryBlog.Client";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "jamie@example.com",
            DisplayName = "Jamie Baker",
            Role = "Author"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = secret,
                ["Jwt:Issuer"] = issuer,
                ["Jwt:Audience"] = audience,
                ["Jwt:AccessTokenMinutes"] = "15"
            })
            .Build();
        var result = new JwtService(configuration).GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        handler.MapInboundClaims = false;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var principal = handler.ValidateToken(
            result.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.Zero
            },
            out var validatedToken);

        var parsedToken = (JwtSecurityToken)validatedToken;
        Assert.Equal(
            SecurityAlgorithms.HmacSha256,
            parsedToken.Header.Alg);
        Assert.Equal(
            user.Id.ToString(),
            parsedToken.Claims.First(claim =>
                claim.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("Author", principal.FindFirst("role")?.Value);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    private sealed class FakeAuthService : IAuthService
    {
        public bool RegisterCalled { get; private set; }

        public Task<AuthUserDto> RegisterAsync(
            string displayName,
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            RegisterCalled = true;
            return Task.FromResult(
                new AuthUserDto(Guid.NewGuid(), email, displayName, "Author"));
        }

        public Task<AuthResponseDto> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken) =>
            Task.FromResult(
                new AuthResponseDto(
                    "token",
                    "Bearer",
                    DateTime.UtcNow.AddMinutes(5),
                    new AuthUserDto(
                        Guid.NewGuid(),
                        email,
                        "Test User",
                        "Author")));
    }
}
