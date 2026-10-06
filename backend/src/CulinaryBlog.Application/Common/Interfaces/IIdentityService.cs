using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<ApplicationUser> RegisterAsync(
        string fullName,
        string email,
        string userName,
        string password,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt,
        CancellationToken cancellationToken = default);
}
