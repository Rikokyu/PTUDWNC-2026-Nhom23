using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly CulinaryBlogDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<IdentityService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
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

    public async Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Id == userId,
                cancellationToken)
            ?? throw new NotFoundException("User account was not found.");

        if (_passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                currentPassword) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("The current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Email.ToLower() == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            return;
        }

        var token = CreatePasswordResetToken(user);
        var frontendUrl = _configuration["PasswordReset:FrontendUrl"]
            ?? "http://localhost:3000/auth/reset-password";
        var separator = frontendUrl.Contains('?') ? "&" : "?";
        var resetUrl = $"{frontendUrl}{separator}email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.DisplayName,
                resetUrl,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Could not deliver password reset email to {Email}.",
                user.Email);
        }
    }

    public async Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Email.ToLower() == normalizedEmail,
                cancellationToken)
            ?? throw new UnauthorizedException("The reset link is invalid or expired.");

        if (!ValidatePasswordResetToken(user, token))
        {
            throw new UnauthorizedException("The reset link is invalid or expired.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        string? bio,
        string? avatarUrl,
        CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Id == userId,
                cancellationToken)
            ?? throw new NotFoundException("User account was not found.");

        user.DisplayName = displayName.Trim();
        user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl)
            ? null
            : avatarUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.Bio,
            user.Role,
            user.CreatedAt);
    }

    private string CreatePasswordResetToken(ApplicationUser user)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(
            new PasswordResetTokenPayload(
                user.Id,
                user.Email,
                GetPasswordHashFingerprint(user.PasswordHash),
                DateTime.UtcNow.AddMinutes(30),
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))));
        var encodedPayload = Base64UrlEncode(payload);
        var signature = HMACSHA256.HashData(
            GetPasswordResetSigningKey(),
            Encoding.UTF8.GetBytes(encodedPayload));

        return $"{encodedPayload}.{Base64UrlEncode(signature)}";
    }

    private bool ValidatePasswordResetToken(ApplicationUser user, string token)
    {
        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var signature = Base64UrlDecode(parts[1]);
            var expectedSignature = HMACSHA256.HashData(
                GetPasswordResetSigningKey(),
                Encoding.UTF8.GetBytes(parts[0]));
            if (signature.Length != expectedSignature.Length
                || !CryptographicOperations.FixedTimeEquals(
                    signature,
                    expectedSignature))
            {
                return false;
            }

            var payload = JsonSerializer.Deserialize<PasswordResetTokenPayload>(
                Base64UrlDecode(parts[0]));
            return payload is not null
                && payload.UserId == user.Id
                && string.Equals(payload.Email, user.Email, StringComparison.Ordinal)
                && string.Equals(
                    payload.PasswordHashFingerprint,
                    GetPasswordHashFingerprint(user.PasswordHash),
                    StringComparison.Ordinal)
                && payload.ExpiresAtUtc > DateTime.UtcNow;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private byte[] GetPasswordResetSigningKey()
    {
        var secret = _configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret)
            || Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be configured with at least 32 UTF-8 bytes.");
        }

        return Encoding.UTF8.GetBytes(secret);
    }

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');
        return Convert.FromBase64String(base64);
    }

    private static string GetPasswordHashFingerprint(string passwordHash) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)));

    private sealed record PasswordResetTokenPayload(
        Guid UserId,
        string Email,
        string PasswordHashFingerprint,
        DateTime ExpiresAtUtc,
        string Nonce);

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
            new Claim("role", user.Role)
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
