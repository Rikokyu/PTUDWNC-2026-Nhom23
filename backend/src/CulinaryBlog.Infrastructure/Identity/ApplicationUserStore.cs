using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class ApplicationUserStore
    : IUserPasswordStore<ApplicationUser>,
      IUserEmailStore<ApplicationUser>,
      IUserRoleStore<ApplicationUser>
{
    private readonly CulinaryBlogDbContext _context;

    public ApplicationUserStore(CulinaryBlogDbContext context)
    {
        _context = context;
    }

    public void Dispose()
    {
    }

    public Task<string> GetUserIdAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.UserName);

    public Task SetUserNameAsync(
        ApplicationUser user,
        string? userName,
        CancellationToken cancellationToken)
    {
        user.UserName = userName?.Trim() ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult<string?>(Normalize(user.UserName));

    public Task SetNormalizedUserNameAsync(
        ApplicationUser user,
        string? normalizedName,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public async Task<IdentityResult> CreateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.ApplicationUsers.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateException exception)
            when (IsUniqueViolation(exception, out var constraintName))
        {
            _context.Entry(user).State = EntityState.Detached;
            return IdentityResult.Failed(CreateDuplicateError(constraintName));
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException(
                "Không thể lưu tài khoản vào cơ sở dữ liệu.",
                exception);
        }
    }

    public async Task<IdentityResult> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
        catch (DbUpdateException exception)
            when (IsUniqueViolation(exception, out var constraintName))
        {
            return IdentityResult.Failed(CreateDuplicateError(constraintName));
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException(
                "Không thể cập nhật tài khoản trong cơ sở dữ liệu.",
                exception);
        }
    }

    public async Task<IdentityResult> DeleteAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.ApplicationUsers.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public Task<ApplicationUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var id))
        {
            return Task.FromResult<ApplicationUser?>(null);
        }

        return _context.ApplicationUsers.SingleOrDefaultAsync(
            user => user.Id == id,
            cancellationToken);
    }

    public Task<ApplicationUser?> FindByNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken) =>
        _context.ApplicationUsers.SingleOrDefaultAsync(
            user => user.UserName.ToUpper() == normalizedUserName,
            cancellationToken);

    public Task SetPasswordHashAsync(
        ApplicationUser user,
        string? passwordHash,
        CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.PasswordHash);

    public Task<bool> HasPasswordAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));

    public Task SetEmailAsync(
        ApplicationUser user,
        string? email,
        CancellationToken cancellationToken)
    {
        user.Email = email?.Trim().ToLowerInvariant() ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Email);

    public Task<bool> GetEmailConfirmedAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task SetEmailConfirmedAsync(
        ApplicationUser user,
        bool confirmed,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<ApplicationUser?> FindByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken) =>
        _context.ApplicationUsers.SingleOrDefaultAsync(
            user => user.Email.ToUpper() == normalizedEmail,
            cancellationToken);

    public Task<string?> GetNormalizedEmailAsync(
        ApplicationUser user,
        CancellationToken cancellationToken) =>
        Task.FromResult<string?>(Normalize(user.Email));

    public Task SetNormalizedEmailAsync(
        ApplicationUser user,
        string? normalizedEmail,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task AddToRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken)
    {
        user.Role = ToDisplayRole(roleName);
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken)
    {
        if (string.Equals(
                Normalize(user.Role),
                roleName,
                StringComparison.Ordinal))
        {
            user.Role = string.Empty;
        }

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        IList<string> roles = string.IsNullOrWhiteSpace(user.Role)
            ? []
            : [user.Role];
        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken) =>
        Task.FromResult(string.Equals(
            Normalize(user.Role),
            roleName,
            StringComparison.Ordinal));

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(
        string roleName,
        CancellationToken cancellationToken) =>
        await _context.ApplicationUsers
            .Where(user => user.Role.ToUpper() == roleName)
            .ToListAsync(cancellationToken);

    private static string Normalize(string value) =>
        value.ToUpperInvariant();

    private static string ToDisplayRole(string normalizedRole) =>
        string.Equals(normalizedRole, "ADMIN", StringComparison.OrdinalIgnoreCase)
            ? "Admin"
            : string.Equals(normalizedRole, "AUTHOR", StringComparison.OrdinalIgnoreCase)
                ? "Author"
                : normalizedRole;

    private static bool IsUniqueViolation(
        DbUpdateException exception,
        out string? constraintName)
    {
        if (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            constraintName = postgres.ConstraintName;
            return true;
        }

        constraintName = null;
        return false;
    }

    private static IdentityError CreateDuplicateError(string? constraintName) =>
        constraintName?.Contains("Email", StringComparison.OrdinalIgnoreCase) == true
            ? new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "Email đã được đăng ký."
            }
            : new IdentityError
            {
                Code = "DuplicateUserName",
                Description = "Tên đăng nhập đã được sử dụng."
            };
}
