using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Application.Features.Auth.Queries.GetCurrentUser;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class GetCurrentUserQueryTests
{
    [Fact]
    public async Task Handler_ReturnsCurrentUserFromIdentityService()
    {
        var userId = Guid.NewGuid();
        var expectedUser = new UserDto(
            userId,
            "user@example.com",
            "Test User",
            null,
            null,
            "Author",
            DateTime.UtcNow);
        var identityService = new FakeIdentityService(expectedUser);
        var handler = new GetCurrentUserQueryHandler(
            new FakeCurrentUser(userId),
            identityService);

        var result = await handler.Handle(
            new GetCurrentUserQuery(),
            CancellationToken.None);

        Assert.Equal(expectedUser, result);
        Assert.Equal(userId, identityService.RequestedUserId);
    }

    [Fact]
    public async Task Handler_RejectsRequestWithoutAuthenticatedUserId()
    {
        var handler = new GetCurrentUserQueryHandler(
            new FakeCurrentUser(null),
            new FakeIdentityService(null));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(
                new GetCurrentUserQuery(),
                CancellationToken.None));
    }

    private sealed class FakeCurrentUser(Guid? userId) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public bool IsAuthor => IsAuthenticated;
        public bool IsAdmin => false;
    }

    private sealed class FakeIdentityService(UserDto? user)
        : IIdentityService
    {
        public Guid? RequestedUserId { get; private set; }

        public Task<AuthSessionDto> RegisterAsync(
            string displayName,
            string email,
            string password,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<AuthSessionDto?> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserDto?> GetUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            RequestedUserId = userId;
            return Task.FromResult(
                user?.Id == userId ? user : null);
        }

        public Task<UserDto> UpdateProfileAsync(
            Guid userId,
            string displayName,
            string? avatarUrl,
            string? bio,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task ChangePasswordAsync(
            Guid userId,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task RequestPasswordResetAsync(
            string email,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserDto> UpdateProfileAsync(
            Guid userId,
            string displayName,
            string? bio,
            string? avatarUrl,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task RevokeRefreshTokensAsync(
            Guid userId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
