using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthSessionDto> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthSessionDto?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<UserDto?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken);

    Task RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken);

    Task ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken);

    Task<UserDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        string? bio,
        string? avatarUrl,
        CancellationToken cancellationToken);

    Task RevokeRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<AuthSessionDto?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    Task LogoutAsync(Guid userId, string? refreshToken, CancellationToken cancellationToken);

    Task<AuthSessionDto> GoogleLoginAsync(string idToken, CancellationToken cancellationToken);

    Task<UserDto> PatchProfileAsync(Guid userId, bool hasDisplayName, string? displayName,
        bool hasAvatarUrl, string? avatarUrl, bool hasBio, string? bio,
        CancellationToken cancellationToken);
}
