using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.Authentication;

public sealed class JwtService(IConfiguration configuration) : IJwtService
{
    public AccessTokenResult GenerateAccessToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(
            configuration.GetValue("Jwt:AccessTokenMinutes", 15));
        var issuer = configuration["Jwt:Issuer"] ?? "CulinaryBlog";
        var audience = configuration["Jwt:Audience"] ?? "CulinaryBlog";
        var secret = GetSecret();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.UserName)
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            now,
            expiresAt,
            signingCredentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var expiresAt = DateTime.UtcNow.AddDays(
            configuration.GetValue("Jwt:RefreshTokenDays", 7));
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return new RefreshTokenResult(token, expiresAt);
    }

    private string GetSecret()
    {
        var secret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret)
            || Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must contain at least 32 UTF-8 bytes.");
        }

        return secret;
    }
}
