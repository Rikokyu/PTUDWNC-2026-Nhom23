namespace CulinaryBlog.Application.DTOs.Auth;

/// <summary>Public profile information returned after authentication.</summary>
public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role);

/// <summary>Bearer access token and authenticated user profile.</summary>
public sealed record AuthResponseDto(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    AuthUserDto User);

/// <summary>Generated signed access token and its UTC expiration time.</summary>
public sealed record JwtTokenDto(
    string AccessToken,
    DateTime ExpiresAt);
