namespace CulinaryBlog.Application.DTOs.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    AuthUserDto User);

public sealed record AuthUserDto(
    Guid Id,
    string FullName,
    string Email,
    string UserName,
    string? AvatarUrl,
    IReadOnlyList<string> Roles);
