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
        string email,
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
}
