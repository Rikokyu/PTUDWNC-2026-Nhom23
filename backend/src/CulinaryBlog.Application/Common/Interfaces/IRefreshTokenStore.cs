namespace CulinaryBlog.Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    Task StoreAsync(
        Guid userId,
        string refreshToken,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);
}
