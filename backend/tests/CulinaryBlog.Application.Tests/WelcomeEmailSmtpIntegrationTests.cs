using CulinaryBlog.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class WelcomeEmailSmtpIntegrationTests
{
    [Fact]
    public async Task IsolatedMailpit_CapturesWelcomeEmail()
    {
        var smtpPort = Environment.GetEnvironmentVariable("CULINARY_TEST_SMTP_PORT");
        var smtpHost = Environment.GetEnvironmentVariable("CULINARY_TEST_SMTP_HOST");
        var api = Environment.GetEnvironmentVariable("CULINARY_TEST_MAILPIT_API");
        if (smtpPort is null || api is null) return;

        var settings = new Dictionary<string, string?>
        {
            ["Smtp:Host"] = smtpHost ?? "127.0.0.1",
            ["Smtp:Port"] = smtpPort,
            ["Smtp:UseSsl"] = "false",
            ["Smtp:FromAddress"] = "no-reply@culinary.test",
            ["WelcomeEmail:ApplicationUrl"] = "https://culinary.test"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings).Build();
        var service = new MailKitEmailService(configuration,
            NullLogger<MailKitEmailService>.Instance);
        var address = $"user-{Guid.NewGuid():N}@culinary.test";
        await service.SendWelcomeEmailAsync(address, "Test User");

        using var http = new HttpClient();
        var messages = await http.GetStringAsync($"{api}/api/v1/messages");
        Assert.Contains(address, messages);
    }
}
