using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Authentication;

public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenDto GenerateToken(ApplicationUser user)
    {
        var secret = _configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret)
            || Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must contain at least 32 UTF-8 bytes.");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "CulinaryBlog";
        var audience = _configuration["Jwt:Audience"] ?? "CulinaryBlog";
        var lifetimeMinutes =
            _configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 60;
        if (lifetimeMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:AccessTokenMinutes must be a positive integer.");
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(lifetimeMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));
        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            now,
            expiresAt,
            signingCredentials);

        return new JwtTokenDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
