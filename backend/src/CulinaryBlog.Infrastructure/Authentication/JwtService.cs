using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Authentication;

public sealed class JwtService : IJwtService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string? _secret;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public JwtService(IConfiguration configuration)
    {
        _issuer = configuration["Jwt:Issuer"] ?? "CulinaryBlog";
        _audience = configuration["Jwt:Audience"] ?? "CulinaryBlog";
        _secret = configuration["Jwt:Secret"];
        _accessTokenMinutes =
            configuration.GetValue("Jwt:AccessTokenMinutes", 15);
        _refreshTokenDays =
            configuration.GetValue("Jwt:RefreshTokenDays", 7);
    }

    public AccessTokenResult GenerateAccessToken(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(_secret)
            || Encoding.UTF8.GetByteCount(_secret) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must contain at least 32 bytes.");
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_accessTokenMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.DisplayName),
            new Claim("username", user.UserName),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)))
            .ToLowerInvariant();

    public DateTime GetRefreshTokenExpiration() =>
        DateTime.UtcNow.AddDays(_refreshTokenDays);
}
