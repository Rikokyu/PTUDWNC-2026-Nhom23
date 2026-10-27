using CulinaryBlog.Application.DTOs.Users;
namespace CulinaryBlog.Application.Common.Interfaces;

public interface IAccountService
{
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string confirmPassword, CancellationToken ct);
    Task ForgotPasswordAsync(string email, CancellationToken ct);
    Task ResetPasswordAsync(string email, string token, string newPassword, string confirmPassword, CancellationToken ct);
    Task<UserDto> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct);
    Task<UserDto> UpdateProfileAsync(Guid userId, string displayName, string? bio, string? avatarUrl, CancellationToken ct);
}
