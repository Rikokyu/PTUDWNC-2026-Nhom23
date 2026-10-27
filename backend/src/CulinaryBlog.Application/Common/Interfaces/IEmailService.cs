namespace CulinaryBlog.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string email,
        string displayName,
        CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(
        string email,
        string displayName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
