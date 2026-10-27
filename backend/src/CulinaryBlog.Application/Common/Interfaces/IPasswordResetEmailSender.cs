namespace CulinaryBlog.Application.Common.Interfaces;
public interface IPasswordResetEmailSender
{
    Task SendAsync(string email, string resetLink, CancellationToken ct);
}
