namespace CulinaryBlog.Application.Common.Interfaces;

public sealed record VerifiedGoogleUser(string Subject, string Email, string DisplayName, string? AvatarUrl);

public interface IGoogleTokenVerifier
{
    Task<VerifiedGoogleUser?> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
