using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IJwtService
{
    AccessTokenResult GenerateAccessToken(ApplicationUser user);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);

    DateTime GetRefreshTokenExpiration();
}

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAt);
