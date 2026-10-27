using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class AccountService(
    CulinaryBlogDbContext db, IPasswordHasher<ApplicationUser> hasher,
    IDataProtectionProvider protection, IPasswordResetEmailSender emailSender,
    IConfiguration configuration, TimeProvider clock) : IAccountService
{
    private IDataProtector Protector => protection.CreateProtector("CulinaryBlog.PasswordReset.v1");
    private sealed record ResetTicket(Guid UserId, string Version, DateTimeOffset Expires);
    public static string PasswordVersion(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));

    private static void ValidatePassword(string? password, string? confirmation)
    {
        if (password is null || password.Length is < 8 or > 128
            || !Regex.IsMatch(password, "[a-z]") || !Regex.IsMatch(password, "[A-Z]")
            || !Regex.IsMatch(password, "[0-9]") || !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new ValidationException("Password must have 8-128 characters, uppercase, lowercase, digit and special character.");
        if (password != confirmation) throw new ValidationException("Password confirmation does not match.");
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string confirmPassword, CancellationToken ct)
    {
        ValidatePassword(newPassword, confirmPassword);
        var user = await db.ApplicationUsers.SingleOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new ValidationException("Account unavailable.");
        if (string.IsNullOrEmpty(currentPassword) || currentPassword.Length > 128
            || hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
            throw new ValidationException("Current password is incorrect.");
        await SetPasswordAsync(user, newPassword, ct);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320 || !MailAddress.TryCreate(email, out _))
            throw new ValidationException("A valid email address is required.");
        var baseUrl = configuration["PasswordReset:ApplicationUrl"] ?? configuration["WelcomeEmail:ApplicationUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http"))
            throw new InvalidOperationException("PasswordReset:ApplicationUrl must be configured.");
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.ApplicationUsers.AsNoTracking().SingleOrDefaultAsync(x => x.Email.ToLower() == normalized, ct);
        if (user is null) return;
        var ticket = new ResetTicket(user.Id, PasswordVersion(user.PasswordHash), clock.GetUtcNow().AddMinutes(30));
        var token = Protector.Protect(JsonSerializer.Serialize(ticket));
        var link = $"{baseUrl!.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
        await emailSender.SendAsync(user.Email, link, ct);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword, string confirmPassword, CancellationToken ct)
    {
        ValidatePassword(newPassword, confirmPassword);
        const string invalid = "Reset link is invalid or expired. Request a new link.";
        if (string.IsNullOrWhiteSpace(email) || email.Length > 320 || string.IsNullOrWhiteSpace(token) || token.Length > 4096)
            throw new ValidationException(invalid);
        ResetTicket? ticket;
        try { ticket = JsonSerializer.Deserialize<ResetTicket>(Protector.Unprotect(token)); }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException)
        { throw new ValidationException(invalid); }
        if (ticket is null || ticket.Expires <= clock.GetUtcNow()) throw new ValidationException(invalid);
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.ApplicationUsers.SingleOrDefaultAsync(x => x.Id == ticket.UserId && x.Email.ToLower() == normalized, ct);
        if (user is null || PasswordVersion(user.PasswordHash) != ticket.Version) throw new ValidationException(invalid);
        await SetPasswordAsync(user, newPassword, ct);
    }

    private async Task SetPasswordAsync(ApplicationUser user, string password, CancellationToken ct)
    {
        user.PasswordHash = hasher.HashPassword(user, password);
        user.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        var tokens = await db.RefreshTokens.Where(x => x.UserId == user.Id && x.RevokedAt == null).ToListAsync(ct);
        foreach (var token in tokens) token.RevokedAt = user.UpdatedAt;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new ValidationException("Account changed. Request a new reset link or sign in again."); }
    }

    public async Task<UserDto> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct)
    {
        var user = await db.ApplicationUsers.SingleOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new ValidationException("Account unavailable.");
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return new(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Bio, user.Role, user.CreatedAt);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, string displayName, string? bio, string? avatarUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 100 || (bio?.Length ?? 0) > 1000)
            throw new ValidationException("Display name is required (up to 100 characters); bio is limited to 1000 characters.");
        if (!string.IsNullOrWhiteSpace(avatarUrl)
            && (avatarUrl.Length > 500 || !(avatarUrl.StartsWith("/uploads/avatars/", StringComparison.Ordinal)
                && !avatarUrl.Contains("..") && !avatarUrl.Contains('\\'))
                && (!Uri.TryCreate(avatarUrl, UriKind.Absolute, out var avatar) || !new[] { "http", "https" }.Contains(avatar.Scheme))))
            throw new ValidationException("Avatar must be an HTTP(S) URL up to 500 characters.");
        var user = await db.ApplicationUsers.SingleOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new ValidationException("Account unavailable.");
        user.DisplayName = displayName.Trim(); user.Bio = bio?.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        user.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return new(user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Bio, user.Role, user.CreatedAt);
    }
}
