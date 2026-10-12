namespace CulinaryBlog.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(
        string email,
        string fullName,
        CancellationToken cancellationToken = default);
}
