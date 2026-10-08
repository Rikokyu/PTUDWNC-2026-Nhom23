using CulinaryBlog.Application.DTOs.Auth;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthUserDto> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthResponseDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
