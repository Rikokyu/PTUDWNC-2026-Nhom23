using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using DomainConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Infrastructure.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly CulinaryBlogDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly PasswordHasher<ApplicationUser> _passwordHasher = new();

    public AuthService(
        CulinaryBlogDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthUserDto> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ValidationException("DisplayName is required.");
        }

        var emailValue = email.Trim();
        var normalizedDisplayName = displayName.Trim();
        if (normalizedDisplayName.Length > 100
            || emailValue.Length > 50
            || !MailAddress.TryCreate(emailValue, out var parsedEmail)
            || !string.Equals(
                parsedEmail.Address,
                emailValue,
                StringComparison.OrdinalIgnoreCase)
            || password.Length < 8)
        {
            throw new ValidationException(
                "Display name must not exceed 100 characters, email must be valid and no longer than 50 characters, and password must be at least 8 characters.");
        }

        var normalizedEmail = emailValue.ToUpperInvariant();
        var existing = await _context.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                user => user.Email.ToUpper() == normalizedEmail,
                cancellationToken);
        if (existing)
        {
            throw new DomainConflictException(
                "An account with this email already exists.");
        }

        var userName = emailValue;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = emailValue,
            UserName = userName,
            DisplayName = normalizedDisplayName,
            Role = "Author",
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        await _context.ApplicationUsers.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToUserDto(user);
    }

    public async Task<AuthResponseDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var user = await _context.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                candidate => candidate.Email.ToUpper() == normalizedEmail,
                cancellationToken);

        if (user is null
            || _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var token = _jwtService.GenerateToken(user);
        return new AuthResponseDto(
            token.AccessToken,
            "Bearer",
            token.ExpiresAt,
            ToUserDto(user));
    }

    private static AuthUserDto ToUserDto(ApplicationUser user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role);
}
