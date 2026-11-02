using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class AdminUserService : IAdminUserService
{
    private readonly CulinaryBlogDbContext _context;

    public AdminUserService(CulinaryBlogDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? role,
        CancellationToken cancellationToken)
    {
        if (page <= 0 || pageSize <= 0 || pageSize > 100)
        {
            throw new ValidationException(
                "page must be positive and pageSize must be between 1 and 100.");
        }

        var normalizedRole = NormalizeOptionalRole(role);
        var query = _context.ApplicationUsers.AsNoTracking();

        if (normalizedRole is not null)
        {
            query = query.Where(user => user.Role == normalizedRole);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(user =>
                user.Email.ToLower().Contains(normalizedSearch)
                || user.UserName.ToLower().Contains(normalizedSearch)
                || user.DisplayName.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new AdminUserDto(
                user.Id,
                user.Email,
                user.UserName,
                user.DisplayName,
                user.AvatarUrl,
                user.Role,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserDto>(
            users,
            totalCount,
            page,
            pageSize);
    }

    public async Task<AdminUserDto> SetActiveAsync(
        Guid actingAdminId,
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        if (actingAdminId == userId)
        {
            throw new ValidationException(
                "Administrators cannot disable their own account.");
        }

        var user = await GetUserAsync(userId, cancellationToken);
        if (user.Role != UserRoles.Author)
        {
            throw new ValidationException(
                "Only Author accounts can be enabled or disabled.");
        }

        if (user.IsActive != isActive)
        {
            user.IsActive = isActive;
            InvalidateSessions(user);
            await RevokeRefreshTokensAsync(user.Id, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ToDto(user);
    }

    public async Task<AdminUserDto> SetRoleAsync(
        Guid actingAdminId,
        Guid userId,
        string role,
        CancellationToken cancellationToken)
    {
        var normalizedRole = NormalizeRole(role);
        if (actingAdminId == userId)
        {
            throw new ValidationException(
                "Administrators cannot change their own role.");
        }

        var user = await GetUserAsync(userId, cancellationToken);
        if (user.Role == normalizedRole)
        {
            return ToDto(user);
        }

        if (user.Role == UserRoles.Admin
            && normalizedRole == UserRoles.Author)
        {
            var adminCount = await _context.ApplicationUsers.CountAsync(
                candidate => candidate.Role == UserRoles.Admin
                    && candidate.IsActive,
                cancellationToken);
            if (adminCount <= 1)
            {
                throw new ValidationException(
                    "The last active administrator cannot be demoted.");
            }
        }

        user.Role = normalizedRole;
        InvalidateSessions(user);
        await RevokeRefreshTokensAsync(user.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(user);
    }

    private async Task<ApplicationUser> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await _context.ApplicationUsers.SingleOrDefaultAsync(
            user => user.Id == userId,
            cancellationToken)
        ?? throw new NotFoundException("User account was not found.");

    private async Task RevokeRefreshTokensAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokens = await _context.RefreshTokens
            .Where(token => token.UserId == userId
                && token.RevokedAt == null
                && token.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.UpdatedAt = now;
        }
    }

    private static void InvalidateSessions(ApplicationUser user)
    {
        user.TokenVersion++;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeOptionalRole(string? role) =>
        string.IsNullOrWhiteSpace(role) ? null : NormalizeRole(role);

    private static string NormalizeRole(string role)
    {
        var normalizedRole = role.Trim();
        normalizedRole = normalizedRole.Equals(
            UserRoles.Admin,
            StringComparison.OrdinalIgnoreCase)
            ? UserRoles.Admin
            : normalizedRole.Equals(
                UserRoles.Author,
                StringComparison.OrdinalIgnoreCase)
                ? UserRoles.Author
                : normalizedRole;

        if (!UserRoles.IsSupported(normalizedRole))
        {
            throw new ValidationException(
                "role must be either Author or Admin.");
        }

        return normalizedRole;
    }

    private static AdminUserDto ToDto(ApplicationUser user) =>
        new(
            user.Id,
            user.Email,
            user.UserName,
            user.DisplayName,
            user.AvatarUrl,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
}
