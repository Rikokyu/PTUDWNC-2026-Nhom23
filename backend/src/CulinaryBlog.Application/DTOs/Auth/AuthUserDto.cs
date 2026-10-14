namespace CulinaryBlog.Application.DTOs.Auth;

public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role);
