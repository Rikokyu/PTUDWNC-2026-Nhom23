namespace CulinaryBlog.Application.DTOs.Users;

/// <summary>Account information visible only to administrators.</summary>
public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string UserName,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
