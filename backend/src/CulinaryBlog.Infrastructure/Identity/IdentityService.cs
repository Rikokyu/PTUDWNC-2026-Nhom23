using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly CulinaryBlogDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IAccountService _accounts;

    public IdentityService(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IConfiguration configuration,
        IAccountService accounts)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _accounts = accounts;
    }

    public async Task<AuthSessionDto> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var emailExists = await _context.ApplicationUsers
            .AnyAsync(
                user => user.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            UserName = $"user_{Guid.NewGuid():N}",
            DisplayName = displayName.Trim(),
            Role = "Author",
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        await _context.ApplicationUsers.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthSessionDto?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (user is null
            || _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password) == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<UserDto?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == userId,
                cancellationToken);

        return user is null
            ? null
            : new UserDto(
                user.Id,
                user.Email,
                user.DisplayName,
                user.AvatarUrl,
                user.Bio,
                user.Role,
                user.CreatedAt);
    }

    // Preserve develop's command contracts using the validated account implementation.
    public Task<UserDto> UpdateProfileAsync(Guid userId, string displayName, string? avatarUrl, string? bio, CancellationToken ct)
        => _accounts.UpdateProfileAsync(userId, displayName, bio, avatarUrl, ct);

    public Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct)
        => _accounts.ChangePasswordAsync(userId, currentPassword, newPassword, newPassword, ct);

    public Task RequestPasswordResetAsync(string email, CancellationToken ct)
        => _accounts.ForgotPasswordAsync(email, ct);

    public Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct)
        => _accounts.ResetPasswordAsync(email, token, newPassword, newPassword, ct);

    public async Task RevokeRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var activeTokens = await _context.RefreshTokens
            .Where(token =>
                token.UserId == userId
                && token.RevokedAt == null
                && token.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
            token.UpdatedAt = now;
        }

        if (activeTokens.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthSessionDto> CreateSessionAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessTokenMinutes = _configuration.GetValue(
            "Jwt:AccessTokenMinutes",
            15);
        var refreshTokenDays = _configuration.GetValue(
            "Jwt:RefreshTokenDays",
            7);
        var accessTokenExpiresAt = now.AddMinutes(accessTokenMinutes);
        var refreshTokenExpiresAt = now.AddDays(refreshTokenDays);
        var accessToken = CreateAccessToken(user, now, accessTokenExpiresAt);
        var refreshToken = CreateRefreshToken();

        await _context.RefreshTokens.AddAsync(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = now
            },
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthSessionDto(
            accessToken,
            accessTokenExpiresAt,
            refreshToken,
            refreshTokenExpiresAt,
            ToDto(user));
    }

    private string CreateAccessToken(
        ApplicationUser user,
        DateTime issuedAt,
        DateTime expiresAt)
    {
        var issuer = _configuration["Jwt:Issuer"] ?? "CulinaryBlog";
        var audience = _configuration["Jwt:Audience"] ?? "CulinaryBlog";
        var secret = _configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret)
            || Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be configured with at least 32 UTF-8 bytes.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("role", user.Role),
            new Claim("pwdv", AccountService.PasswordVersion(user.PasswordHash))
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            issuedAt,
            expiresAt,
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string HashToken(string token) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static AuthUserDto ToDto(ApplicationUser user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role);
}
