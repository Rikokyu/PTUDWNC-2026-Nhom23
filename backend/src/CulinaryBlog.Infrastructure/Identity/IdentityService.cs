using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly CulinaryBlogDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public IdentityService(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApplicationUser> RegisterAsync(
        string fullName,
        string email,
        string userName,
        string password,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var normalizedUserName = userName.ToUpperInvariant();

        if (await _context.ApplicationUsers.AnyAsync(
                user => user.Email.ToUpper() == normalizedEmail,
                cancellationToken))
        {
            throw new ConflictException(
                $"Email '{email}' is already registered.");
        }

        if (await _context.ApplicationUsers.AnyAsync(
                user => user.UserName.ToUpper() == normalizedUserName,
                cancellationToken))
        {
            throw new ConflictException(
                $"UserName '{userName}' is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            DisplayName = fullName,
            Role = "Author",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            password);

        user.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        });

        await _context.ApplicationUsers.AddAsync(
            user,
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
