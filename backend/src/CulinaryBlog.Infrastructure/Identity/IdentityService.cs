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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly CulinaryBlogDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly IGoogleTokenVerifier _googleTokenVerifier;
    private readonly ILogger<IdentityService> _logger;
    private readonly ITimeLimitedDataProtector _passwordResetProtector;

    public IdentityService(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IConfiguration configuration,
        IEmailSender emailSender,
        IDataProtectionProvider dataProtectionProvider,
        IGoogleTokenVerifier googleTokenVerifier,
        ILogger<IdentityService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _emailSender = emailSender;
        _googleTokenVerifier = googleTokenVerifier;
        _logger = logger;
        _passwordResetProtector = dataProtectionProvider
            .CreateProtector("CulinaryBlog.PasswordReset.v1")
            .ToTimeLimitedDataProtector();
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

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        if (user.LockoutEnd > now)
            throw new AccountLockedException(user.LockoutEnd.Value);
        if (user.LockoutEnd is not null && user.LockoutEnd <= now)
        {
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password)
            == PasswordVerificationResult.Failed)
        {
            await _context.ApplicationUsers.Where(candidate => candidate.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.AccessFailedCount, candidate => candidate.AccessFailedCount + 1)
                    .SetProperty(candidate => candidate.LockoutEnd, candidate =>
                        candidate.AccessFailedCount + 1 >= 5 ? now.AddMinutes(15) : null),
                    cancellationToken);
            var lockoutEnd = await _context.ApplicationUsers.AsNoTracking()
                .Where(candidate => candidate.Id == user.Id)
                .Select(candidate => candidate.LockoutEnd).SingleAsync(cancellationToken);
            if (lockoutEnd > now)
                throw new AccountLockedException(lockoutEnd.Value);
            return null;
        }

        if (user.AccessFailedCount != 0 || user.LockoutEnd is not null)
            await _context.ApplicationUsers.Where(candidate => candidate.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(candidate => candidate.AccessFailedCount, 0)
                    .SetProperty(candidate => candidate.LockoutEnd, (DateTime?)null), cancellationToken);

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
                user.IsActive,
                user.CreatedAt);
    }

    public async Task<UserDto> UpdateProfileAsync(
        Guid userId,
        string displayName,
        string? bio,
        string? avatarUrl,
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
            user.IsActive,
            user.CreatedAt);

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

    public async Task<AuthSessionDto?> RefreshAsync(
        string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length > 512)
            return null;

        var hash = HashToken(refreshToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        // A row lock serializes rotation and reuse detection across API instances.
        var token = await _context.RefreshTokens
            .FromSqlInterpolated($"SELECT * FROM \"RefreshTokens\" WHERE \"TokenHash\" = {hash} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (token is null)
            return null;

        var now = DateTime.UtcNow;
        if (token.ReplacedByTokenHash is not null)
        {
            await _context.RefreshTokens.Where(item => item.FamilyId == token.FamilyId && item.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.RevokedAt, now)
                    .SetProperty(item => item.UpdatedAt, now), cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogWarning("Refresh token reuse detected for family {FamilyId}", token.FamilyId);
            return null;
        }

        if (token.RevokedAt is not null || token.ExpiresAt <= now)
            return null;
        var user = await _context.ApplicationUsers.SingleOrDefaultAsync(
            candidate => candidate.Id == token.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.LockoutEnd > now)
            return null;

        var rawNext = CreateRefreshToken();
        var nextHash = HashToken(rawNext);
        var accessExpires = now.AddMinutes(_configuration.GetValue("Jwt:AccessTokenMinutes", 15));
        var refreshExpires = now.AddDays(_configuration.GetValue("Jwt:RefreshTokenDays", 7));
        token.RevokedAt = now;
        token.ReplacedByTokenHash = nextHash;
        token.UpdatedAt = now;
        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = nextHash,
            FamilyId = token.FamilyId, ExpiresAt = refreshExpires, CreatedAt = now
        });
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new AuthSessionDto(CreateAccessToken(user, now, accessExpires),
            accessExpires, rawNext, refreshExpires, ToDto(user));
    }

    public async Task LogoutAsync(Guid userId, string? refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length > 512)
            return;
        var now = DateTime.UtcNow;
        await _context.RefreshTokens.Where(token => token.UserId == userId
                && token.TokenHash == HashToken(refreshToken) && token.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAt, now)
                .SetProperty(token => token.UpdatedAt, now), cancellationToken);
    }

    public async Task<AuthSessionDto> GoogleLoginAsync(string idToken,
        CancellationToken cancellationToken)
    {
        var profile = await _googleTokenVerifier.VerifyAsync(idToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid Google ID token.");
        var user = await _context.ApplicationUsers.SingleOrDefaultAsync(
            candidate => candidate.GoogleSubject == profile.Subject, cancellationToken);
        if (user is null)
        {
            user = await _context.ApplicationUsers.SingleOrDefaultAsync(
                candidate => candidate.Email == profile.Email, cancellationToken);
            if (user is not null && (!user.IsActive || user.LockoutEnd > DateTime.UtcNow))
                throw new UnauthorizedException("Account is unavailable.");
            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(), Email = profile.Email,
                    UserName = $"user_{Guid.NewGuid():N}",
                    DisplayName = profile.DisplayName.Trim()[..Math.Min(profile.DisplayName.Trim().Length, 100)],
                    AvatarUrl = profile.AvatarUrl, GoogleSubject = profile.Subject,
                    Role = "Author", CreatedAt = DateTime.UtcNow,
                    PasswordHash = string.Empty
                };
                _context.ApplicationUsers.Add(user);
            }
            else if (user.GoogleSubject is null)
                user.GoogleSubject = profile.Subject;
            else
                throw new UnauthorizedException("Google account does not match the existing account.");
            await _context.SaveChangesAsync(cancellationToken);
        }
        if (!user.IsActive || user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedException("Account is unavailable.");
        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<UserDto> PatchProfileAsync(Guid userId, bool hasDisplayName,
        string? displayName, bool hasAvatarUrl, string? avatarUrl, bool hasBio,
        string? bio, CancellationToken cancellationToken)
    {
        var user = await _context.ApplicationUsers.SingleOrDefaultAsync(
            candidate => candidate.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User profile was not found.");
        if (hasDisplayName)
        {
            var name = displayName?.Trim();
            if (name is null || name.Length is < 2 or > 100)
                throw new ValidationException("Display name must contain between 2 and 100 characters.", "displayName");
            user.DisplayName = name;
        }
        if (hasAvatarUrl)
        {
            var url = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
            if (url is not null && (url.Length > 500 || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || !string.IsNullOrEmpty(uri.UserInfo)))
                throw new ValidationException("Avatar URL must be an absolute HTTP or HTTPS URL without embedded credentials.", "avatarUrl");
            user.AvatarUrl = url;
        }
        if (hasBio)
        {
            if (bio?.Trim().Length > 500)
                throw new ValidationException("Bio must be at most 500 characters.", "bio");
            user.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        }
        if (hasDisplayName || hasAvatarUrl || hasBio)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return ToProfileDto(user);
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
                FamilyId = Guid.NewGuid(),
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
            new Claim("token_version", user.TokenVersion.ToString())
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
