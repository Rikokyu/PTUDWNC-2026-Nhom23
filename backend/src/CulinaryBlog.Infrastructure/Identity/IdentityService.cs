using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly CulinaryBlogDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ITimeLimitedDataProtector _passwordResetProtector;

    public IdentityService(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IConfiguration configuration,
        IEmailSender emailSender,
        IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _emailSender = emailSender;
        _passwordResetProtector = dataProtectionProvider
            .CreateProtector("CulinaryBlog.PasswordReset.v1")
            .ToTimeLimitedDataProtector();
    }

    public async Task<AuthUserDto> RegisterAsync(
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

        return ToDto(user);
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

    public async Task<UserDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        string? avatarUrl,
        string? bio,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ValidationException("Display name is required.");
        }

        var normalizedName = displayName.Trim();
        if (normalizedName.Length is < 2 or > 100)
        {
            throw new ValidationException(
                "Display name must contain between 2 and 100 characters.");
        }

        var normalizedAvatarUrl = string.IsNullOrWhiteSpace(avatarUrl)
            ? null
            : avatarUrl.Trim();
        if (normalizedAvatarUrl is not null)
        {
            if (!Uri.TryCreate(
                    normalizedAvatarUrl,
                    UriKind.Absolute,
                    out var avatarUri)
                || (avatarUri.Scheme != Uri.UriSchemeHttp
                    && avatarUri.Scheme != Uri.UriSchemeHttps)
                || !string.IsNullOrEmpty(avatarUri.UserInfo))
            {
                throw new ValidationException(
                    "Avatar URL must be an absolute HTTP or HTTPS URL without embedded credentials.");
            }
        }

        if (bio?.Trim().Length > 500)
        {
            throw new ValidationException("Bio must be at most 500 characters.");
        }

        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User profile was not found.");
        user.DisplayName = normalizedName;
        user.AvatarUrl = normalizedAvatarUrl;
        user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return ToProfileDto(user);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User account was not found.");

        if (string.IsNullOrEmpty(currentPassword))
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        if (_passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                currentPassword) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Current password is incorrect.");
        }

        ValidatePassword(newPassword);
        if (_passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                newPassword) != PasswordVerificationResult.Failed)
        {
            throw new ValidationException(
                "The new password must be different from the current password.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await RevokeActiveTokensAsync(userId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        if (!_emailSender.IsConfigured)
        {
            throw new EmailServiceUnavailableException(
                "Password recovery email is not configured.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Email.ToLower() == normalizedEmail,
                cancellationToken);
        if (user is null)
        {
            return;
        }

        var tokenPayload = JsonSerializer.Serialize(
            new PasswordResetPayload(user.Id, user.Email, user.PasswordHash));
        var token = _passwordResetProtector.Protect(
            tokenPayload,
            TimeSpan.FromMinutes(60));
        var frontendUrl = _configuration["Frontend:BaseUrl"]
            ?? "http://localhost:3000";
        var resetUrl = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
        var htmlBody = $"<p>Yêu cầu đặt lại mật khẩu cho CulinaryBlog.</p><p><a href=\"{System.Net.WebUtility.HtmlEncode(resetUrl)}\">Đặt lại mật khẩu</a></p><p>Liên kết hết hạn sau 60 phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>";
        await _emailSender.SendAsync(
            user.Email,
            "Đặt lại mật khẩu CulinaryBlog",
            htmlBody,
            cancellationToken);
    }

    public async Task ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        ValidatePassword(newPassword);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ValidationException("Password reset token is invalid or expired.");
        }

        PasswordResetPayload payload;
        try
        {
            var json = _passwordResetProtector.Unprotect(token, out _);
            payload = JsonSerializer.Deserialize<PasswordResetPayload>(json)
                ?? throw new ValidationException("Password reset token is invalid or expired.");
        }
        catch (Exception exception) when (
            exception is CryptographicException or JsonException)
        {
            throw new ValidationException("Password reset token is invalid or expired.");
        }

        var user = await _context.ApplicationUsers
            .SingleOrDefaultAsync(
                candidate => candidate.Id == payload.UserId,
                cancellationToken);
        if (user is null
            || !string.Equals(user.Email, payload.Email, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(user.PasswordHash, payload.PasswordHash, StringComparison.Ordinal))
        {
            throw new ValidationException("Password reset token is invalid or expired.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await RevokeActiveTokensAsync(user.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeActiveTokensAsync(
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
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password)
            || password.Length < 8
            || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            throw new ValidationException(
                "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");
        }
    }

    private static UserDto ToProfileDto(ApplicationUser user) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.Bio,
            user.Role,
            user.CreatedAt);

    private sealed record PasswordResetPayload(
        Guid UserId,
        string Email,
        string PasswordHash);

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
