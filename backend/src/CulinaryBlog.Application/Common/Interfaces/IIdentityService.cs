using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthUserDto> RegisterAsync(
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

    Task<UserDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        string? avatarUrl,
        string? bio,
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

    Task RevokeRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
