namespace CulinaryBlog.Application.DTOs.Users;

/// <summary>Public profile fields for an account.</summary>
public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string Role,
    DateTime CreatedAt);