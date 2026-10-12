using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using DomainConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private const string DefaultRole = "Author";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenStore _refreshTokens;
    private readonly IWelcomeEmailJobScheduler _welcomeEmails;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IRefreshTokenStore refreshTokens,
        IWelcomeEmailJobScheduler welcomeEmails)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _welcomeEmails = welcomeEmails;
    }

    public async Task<AuthResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new DomainConflictException("Email đã được đăng ký.");
        }

        if (await _userManager.FindByNameAsync(userName) is not null)
        {
            throw new DomainConflictException("Tên đăng nhập đã được sử dụng.");
        }

        var user = ApplicationUser.Create(
            request.FullName,
            email,
            userName);

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);
        EnsureIdentitySucceeded(createResult);

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            DefaultRole);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            EnsureIdentitySucceeded(roleResult);
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokens.StoreAsync(
            user.Id,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            cancellationToken);

        _welcomeEmails.Enqueue(user.Email, user.DisplayName);

        return new AuthResponseDto(
            accessToken.Token,
            refreshToken.Token,
            accessToken.ExpiresAt,
            new AuthUserDto(
                user.Id,
                user.DisplayName,
                user.Email,
                user.UserName,
                user.AvatarUrl,
                roles));
    }

    private static void EnsureIdentitySucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var duplicate = result.Errors.FirstOrDefault(error =>
            error.Code.Contains("DuplicateEmail", StringComparison.OrdinalIgnoreCase)
            || error.Code.Contains("DuplicateUserName", StringComparison.OrdinalIgnoreCase));
        if (duplicate is not null)
        {
            throw new DomainConflictException(duplicate.Description);
        }

        var errors = result.Errors
            .GroupBy(
                error => ResolveField(error.Code),
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        throw new ValidationException(errors);
    }

    private static string ResolveField(string identityErrorCode) =>
        identityErrorCode.Contains("Password", StringComparison.OrdinalIgnoreCase)
            ? "password"
            : identityErrorCode.Contains("UserName", StringComparison.OrdinalIgnoreCase)
                ? "userName"
                : identityErrorCode.Contains("Email", StringComparison.OrdinalIgnoreCase)
                    ? "email"
                    : "request";
}
