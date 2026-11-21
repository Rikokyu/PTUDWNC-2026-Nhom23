using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Application.Features.Auth.Commands.GoogleLogin;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class GoogleLoginCommandHandlerTests
{
    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task Welcome_Email_Is_Queued_Only_For_New_Google_Account(
        bool isNewUser, int expectedJobs)
    {
        var user = new AuthUserDto(Guid.NewGuid(), "google@example.test", "Google User", "Author");
        var session = new AuthSessionDto("access", DateTime.UtcNow.AddMinutes(15),
            "refresh", DateTime.UtcNow.AddDays(7), user);
        var scheduler = new RecordingWelcomeEmailScheduler();
        var handler = new GoogleLoginCommandHandler(
            new StubIdentityService(new GoogleLoginResult(session, isNewUser)), scheduler);

        var result = await handler.Handle(new GoogleLoginCommand("verified-id-token"), default);

        Assert.Same(session, result);
        Assert.Equal(expectedJobs, scheduler.Jobs.Count);
        if (isNewUser)
            Assert.Equal((user.Email, user.DisplayName), Assert.Single(scheduler.Jobs));
    }

    private sealed class RecordingWelcomeEmailScheduler : IWelcomeEmailJobScheduler
    {
        public List<(string Email, string DisplayName)> Jobs { get; } = [];
        public void Enqueue(string email, string displayName) => Jobs.Add((email, displayName));
    }

    private sealed class StubIdentityService(GoogleLoginResult googleResult) : IIdentityService
    {
        public Task<GoogleLoginResult> GoogleLoginAsync(string idToken,
            CancellationToken cancellationToken) => Task.FromResult(googleResult);
        public Task<AuthSessionDto> RegisterAsync(string displayName, string email,
            string password, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AuthSessionDto?> LoginAsync(string email, string password,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<UserDto?> GetUserByIdAsync(Guid userId,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task ChangePasswordAsync(Guid userId, string currentPassword,
            string newPassword, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task RequestPasswordResetAsync(string email,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task ResetPasswordAsync(string token, string newPassword,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<UserDto> UpdateProfileAsync(Guid userId, string displayName,
            string? bio, string? avatarUrl, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task RevokeRefreshTokensAsync(Guid userId,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AuthSessionDto?> RefreshAsync(string refreshToken,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task LogoutAsync(Guid userId, string? refreshToken,
            CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<UserDto> PatchProfileAsync(Guid userId, bool hasDisplayName,
            string? displayName, bool hasAvatarUrl, string? avatarUrl, bool hasBio,
            string? bio, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
