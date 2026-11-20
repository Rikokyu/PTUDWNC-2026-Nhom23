using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Email;

public sealed class MailKitEmailService(
    IConfiguration configuration,
    ILogger<MailKitEmailService> logger) : IEmailService
{
    public async Task SendWelcomeEmailAsync(
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException(
                "Smtp:Host is required to send welcome emails.");
        }

        var fromAddress = configuration["Smtp:FromAddress"];
        if (string.IsNullOrWhiteSpace(fromAddress))
        {
            throw new InvalidOperationException(
                "Smtp:FromAddress is required to send welcome emails.");
        }
        var fromName = configuration["Smtp:FromName"]
            ?? "Culinary Blog";
        var applicationUrl = configuration["WelcomeEmail:ApplicationUrl"]
            ?? "http://localhost:3000";
        var activationUrl = configuration.GetValue<bool>(
                "WelcomeEmail:RequireActivation")
            ? configuration["WelcomeEmail:ActivationUrl"]
                ?? throw new InvalidOperationException(
                    "WelcomeEmail:ActivationUrl is required when email activation is enabled.")
            : null;
        var content = WelcomeEmailTemplate.Create(
            displayName,
            applicationUrl,
            activationUrl);

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = content.Subject,
            SubjectEncoding = Encoding.UTF8,
            Body = $"Xin chào {displayName},\n\nTài khoản Culinary Blog của bạn đã được tạo thành công.\n{(activationUrl ?? applicationUrl)}",
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(email, displayName));

        using var logoStream = typeof(MailKitEmailService).Assembly.GetManifestResourceStream(
            "CulinaryBlog.Infrastructure.Email.Assets.logo.png")
            ?? throw new InvalidOperationException("Welcome email logo resource is missing.");
        var htmlView = AlternateView.CreateAlternateViewFromString(
            content.HtmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
        htmlView.LinkedResources.Add(new LinkedResource(logoStream, MediaTypeNames.Image.Png)
        {
            ContentId = WelcomeEmailTemplate.LogoContentId
        });
        message.AlternateViews.Add(htmlView);

#pragma warning disable SYSLIB0014
        using var client = new SmtpClient(
            host,
            configuration.GetValue("Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Smtp:UseSsl", true)
        };
#pragma warning restore SYSLIB0014

        var userName = configuration["Smtp:UserName"];
        var password = configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(userName))
        {
            client.Credentials = new NetworkCredential(userName, password);
        }

        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation("Sent welcome email to {Email}.", email);
    }
    public async Task SendPasswordResetEmailAsync(
        string email,
        string displayName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException(
                "Smtp:Host is required to send password reset emails.");
        }

        var fromAddress = configuration["Smtp:FromAddress"]
            ?? "no-reply@culinary.local";
        var fromName = configuration["Smtp:FromName"]
            ?? "Culinary Blog";
        var safeName = WebUtility.HtmlEncode(displayName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);
        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = "Đặt lại mật khẩu Culinary Blog",
            Body = $"<p>Xin chào {safeName},</p><p>Nhấn vào liên kết sau để đặt lại mật khẩu. Liên kết có hiệu lực trong 30 phút.</p><p><a href=\"{safeUrl}\">Đặt lại mật khẩu</a></p><p>Nếu bạn không yêu cầu thay đổi này, hãy bỏ qua email.</p>",
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(email, displayName));

#pragma warning disable SYSLIB0014
        using var client = new SmtpClient(
            host,
            configuration.GetValue("Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Smtp:UseSsl", true)
        };
#pragma warning restore SYSLIB0014

        var userName = configuration["Smtp:UserName"];
        var password = configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(userName))
        {
            client.Credentials = new NetworkCredential(userName, password);
        }

        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation("Sent password reset email to {Email}.", email);
    }
}
