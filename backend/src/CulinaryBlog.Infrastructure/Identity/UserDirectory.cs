using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class UserDirectory(CulinaryBlogDbContext db, ICurrentUser currentUser) : IUserDirectory
{
    public async Task<PagedResult<DirectoryUserDto>> GetAsync(int page, int pageSize, string? search, string? role, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || (!currentUser.IsAdmin && !currentUser.IsAuthor))
            throw new ForbiddenException("Author or Admin access is required.");
        if (page < 1 || pageSize is < 1 or > 50 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new ValidationException("Invalid pagination.");
        if (search?.Length > 100 || (role is not null && role is not ("Author" or "Admin")))
            throw new ValidationException("Invalid directory filters.");
        var query = db.ApplicationUsers.AsNoTracking();
        // SRS grants Authors no administrative data access. Expose only public author profiles.
        if (!currentUser.IsAdmin) query = query.Where(user => user.Role == "Author");
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(user => user.DisplayName.Contains(search));
        if (role is not null) query = query.Where(user => user.Role == role);
        var count = await query.CountAsync(cancellationToken);
        var admin = currentUser.IsAdmin;
        var items = await query.OrderBy(user => user.DisplayName).ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(user => new DirectoryUserDto(user.Id, user.DisplayName, user.AvatarUrl, user.Role, admin ? user.Email : null))
            .ToListAsync(cancellationToken);
        return new(items, count, page, pageSize);
    }
}
