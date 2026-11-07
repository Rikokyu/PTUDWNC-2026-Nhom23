
using System.Net;
using CulinaryBlog.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CulinaryBlog.Infrastructure.Email;

public sealed class MailKitEmailService(
    IConfiguration configuration,
    ILogger<MailKitEmailService> logger
) : IEmailService
{
    public async Task SendWelcomeEmailAsync(
        string email,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        var applicationUrl =
            configuration["WelcomeEmail:ApplicationUrl"]
            ?? "http://localhost:3000";

        var activationUrl =
            configuration.GetValue<bool>(
                "WelcomeEmail:RequireActivation")
            ? configuration["WelcomeEmail:ActivationUrl"]
                ?? throw new InvalidOperationException(
                    "WelcomeEmail:ActivationUrl is required.")
            : null;
        var logoUrl = configuration["WelcomeEmail:LogoUrl"];
        if (string.IsNullOrWhiteSpace(logoUrl))
        {
            logoUrl = $"{applicationUrl.TrimEnd('/')}/icons/logo.png";
        }

        var content = WelcomeEmailTemplate.Create(
            displayName,
            applicationUrl,
            activationUrl,
            logoUrl);

        await SendHtmlEmailAsync(
            email,
            displayName,
            content.Subject,
            content.HtmlBody,
            cancellationToken);

        logger.LogInformation(
            "Sent welcome email to {Email}.", email);
    }

    public async Task SendPasswordResetEmailAsync(
        string email,
        string displayName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var safeName = WebUtility.HtmlEncode(displayName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);

        var html = $"""
            <p>Xin chào {safeName},</p>
            <p>Nhấn vào liên kết sau để đặt lại mật khẩu.</p>
            <p>Liên kết có hiệu lực trong 30 phút.</p>
            <p>
                <a href="{safeUrl}">
                    Đặt lại mật khẩu
                </a>
            </p>
            <p>
                Nếu bạn không yêu cầu thay đổi này,
                hãy bỏ qua email.
            </p>
            """;

        await SendHtmlEmailAsync(
            email,
            displayName,
            "Đặt lại mật khẩu Culinary Blog",
            html,
            cancellationToken);

        logger.LogInformation(
            "Sent password reset email to {Email}.", email);
    }

    private async Task SendHtmlEmailAsync(
        string email,
        string displayName,
        string subject,
        string html,
        CancellationToken cancellationToken)
    {
        var host = configuration["Smtp:Host"]
            ?? throw new InvalidOperationException(
                "Smtp:Host is required.");

        var port = configuration.GetValue(
            "Smtp:Port", 587);

        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SMTP username or password is missing.");
        }

        var fromAddress =
            configuration["Smtp:FromAddress"]
            ?? throw new InvalidOperationException(
                "Smtp:FromAddress is required.");

        var fromName =
            configuration["Smtp:FromName"]
            ?? "Culinary Blog";

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(fromName, fromAddress));

        message.To.Add(
            new MailboxAddress(displayName, email));

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = html
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        // Gmail port 587 sử dụng STARTTLS.
        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        // Đăng nhập SMTP bằng App Password.
        await client.AuthenticateAsync(
            username,
            password,
            cancellationToken);

        // Gửi email.
        await client.SendAsync(
            message,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}
