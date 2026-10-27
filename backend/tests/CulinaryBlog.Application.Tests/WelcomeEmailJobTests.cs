using System.Reflection;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Email;
using Hangfire;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class WelcomeEmailJobTests
{
    [Fact]
    public void Job_UsesRequiredRetrySchedule()
    {
        var method = typeof(WelcomeEmailJob).GetMethod(
            nameof(WelcomeEmailJob.SendAsync));
        var retry = method?.GetCustomAttribute<AutomaticRetryAttribute>();

        Assert.NotNull(retry);
        Assert.Equal(3, retry.Attempts);
        Assert.Equal([60, 300, 1800], retry.DelaysInSeconds);
        Assert.Equal(AttemptsExceededAction.Fail, retry.OnAttemptsExceeded);
        Assert.True(retry.LogEvents);
    }

    [Fact]
    public void Template_ContainsEncodedNameAndRequiredLinks()
    {
        var content = WelcomeEmailTemplate.Create(
            "An <script>alert(1)</script>",
            "http://localhost:3000",
            "http://localhost:3000/auth/activate?token=a&user=1");

        Assert.Contains("An &lt;script&gt;", content.HtmlBody);
        Assert.DoesNotContain("<script>", content.HtmlBody);
        Assert.Contains("http://localhost:3000", content.HtmlBody);
        Assert.Contains("Kích hoạt email", content.HtmlBody);
        Assert.Contains("token=a&amp;user=1", content.HtmlBody);
    }

    [Fact]
    public void Template_OmitsActivationLinkWhenActivationIsNotRequired()
    {
        var content = WelcomeEmailTemplate.Create(
            "Nguyễn Văn An",
            "http://localhost:3000");

        Assert.DoesNotContain("Kích hoạt email", content.HtmlBody);
        Assert.Contains("Đi đến Culinary Blog", content.HtmlBody);
    }
}
