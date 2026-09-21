namespace CulinaryBlog.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool IsSuccess, string UserId, string Token, string[] Errors)> RegisterAsync(string email, string password, string displayName);
    Task<(bool IsSuccess, string UserId, string Token, string[] Errors)> LoginAsync(string email, string password);
    Task<bool> DeleteUserAsync(string userId);
}