using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Users;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        string? role,
        CancellationToken cancellationToken);

    Task<AdminUserDto> SetActiveAsync(
        Guid actingAdminId,
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken);

    Task<AdminUserDto> SetRoleAsync(
        Guid actingAdminId,
        Guid userId,
        string role,
        CancellationToken cancellationToken);
}
