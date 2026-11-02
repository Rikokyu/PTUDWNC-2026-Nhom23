using System.Net;
using System.Net.Mail;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.Identity;

public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(configuration["Email:Smtp:Host"] ?? configuration["Smtp:Host"])
        && !string.IsNullOrWhiteSpace(configuration["Email:Smtp:From"] ?? configuration["Smtp:FromAddress"]);

    public async Task SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        var host = configuration["Email:Smtp:Host"] ?? configuration["Smtp:Host"];
        var from = configuration["Email:Smtp:From"] ?? configuration["Smtp:FromAddress"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            throw new EmailServiceUnavailableException(
                "Password recovery email is unavailable because Email:Smtp:Host and Email:Smtp:From are not configured.");
        }

        var port = configuration.GetValue<int?>("Email:Smtp:Port")
            ?? configuration.GetValue("Smtp:Port", 587);
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = configuration.GetValue<bool?>("Email:Smtp:EnableSsl")
                ?? configuration.GetValue("Smtp:UseSsl", true)
        };
        var username = configuration["Email:Smtp:Username"] ?? configuration["Smtp:UserName"];
        var password = configuration["Email:Smtp:Password"] ?? configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        using var message = new MailMessage(from, recipient, subject, htmlBody)
        {
            IsBodyHtml = true
        };
        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (SmtpException exception)
        {
            logger.LogError(exception, "Password recovery email could not be delivered.");
            throw new EmailServiceUnavailableException(
                "Password recovery email could not be delivered.");
        }
    }
}
