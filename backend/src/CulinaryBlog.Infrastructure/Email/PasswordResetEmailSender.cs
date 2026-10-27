using System.Net;
using System.Net.Mail;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace CulinaryBlog.Infrastructure.Email;

public sealed class PasswordResetEmailSender(IConfiguration configuration, ILogger<PasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    public async Task SendAsync(string email, string resetLink, CancellationToken ct)
    {
        // Never log reset links or SMTP exceptions (which can contain recipient addresses).
        try
        {
            using var message = new MailMessage(configuration["Smtp:FromAddress"] ?? "no-reply@culinary.local", email)
            {
                Subject = "Culinary Blog - Reset password",
                Body = $"Use this link within 30 minutes to reset your password: {resetLink}\nIf you did not request this, ignore this email."
            };
            using var client = new SmtpClient(configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host is required."), configuration.GetValue("Smtp:Port", 587))
            { EnableSsl = configuration.GetValue("Smtp:UseSsl", true) };
            if (!string.IsNullOrWhiteSpace(configuration["Smtp:UserName"]))
                client.Credentials = new NetworkCredential(configuration["Smtp:UserName"], configuration["Smtp:Password"]);
            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            // The HTTP result must not reveal whether an account exists, including delivery failures.
            logger.LogError("Password reset email delivery failed; check SMTP configuration and availability.");
        }
    }
}
