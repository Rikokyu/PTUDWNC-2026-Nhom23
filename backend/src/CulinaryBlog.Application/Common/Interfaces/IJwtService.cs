using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IJwtService
{
    AccessTokenResult GenerateAccessToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles);

    RefreshTokenResult GenerateRefreshToken();
}

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAt);

public sealed record RefreshTokenResult(
    string Token,
    DateTime ExpiresAt);
