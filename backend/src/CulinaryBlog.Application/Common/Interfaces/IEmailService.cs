namespace CulinaryBlog.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string email,
        string displayName,
        CancellationToken cancellationToken = default);
}
