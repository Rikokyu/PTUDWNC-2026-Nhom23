using System.Security.Cryptography;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class RefreshTokenStore(CulinaryBlogDbContext context)
    : IRefreshTokenStore
{
    public async Task StoreAsync(
        Guid userId,
        string refreshToken,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
