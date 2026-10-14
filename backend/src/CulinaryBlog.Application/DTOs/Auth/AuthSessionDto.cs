namespace CulinaryBlog.Application.DTOs.Auth;

public sealed record AuthSessionDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AuthUserDto User);
