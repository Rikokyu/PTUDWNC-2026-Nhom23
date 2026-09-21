using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<(bool IsSuccess, string UserId, string Token, string[] Errors)> RegisterAsync(
        string email, string password, string displayName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return (false, string.Empty, string.Empty, result.Errors.Select(e => e.Description).ToArray());
        }

        string token = _jwtService.GenerateToken(user.Id, user.Email!, user.DisplayName);

        return (true, user.Id, token, Array.Empty<string>());
    }

    public async Task<(bool IsSuccess, string UserId, string Token, string[] Errors)> LoginAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, string.Empty, string.Empty, new[] { "Email hoặc mật khẩu không chính xác." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            return (false, string.Empty, string.Empty, new[] { "Email hoặc mật khẩu không chính xác." });
        }

        string token = _jwtService.GenerateToken(user.Id, user.Email!, user.DisplayName);

        return (true, user.Id, token, Array.Empty<string>());
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}