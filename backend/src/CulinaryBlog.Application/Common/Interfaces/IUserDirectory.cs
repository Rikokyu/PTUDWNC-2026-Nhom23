using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.Common.Interfaces;

public sealed record DirectoryUserDto(Guid Id, string DisplayName, string? AvatarUrl, string Role, string? Email);

public interface IUserDirectory
{
    Task<PagedResult<DirectoryUserDto>> GetAsync(int page, int pageSize, string? search, string? role, CancellationToken cancellationToken);
}
