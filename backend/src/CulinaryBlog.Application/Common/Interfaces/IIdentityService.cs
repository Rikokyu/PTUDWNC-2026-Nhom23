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

    Task RevokeRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
