using CulinaryBlog.Infrastructure.BackgroundJobs;
using Hangfire;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class SitemapGenerationJobTests
{
    [Fact]
    public void BuildUrls_IncludesStaticPagesCategoriesAndRecipesWithoutDuplicates()
    {
        var urls = SitemapGenerationJob.BuildUrls(
            new Uri("https://culinary.example/"),
            ["/", "/recipes", "/categories"],
            ["mon-chinh"],
            ["pho-bo"]);

        Assert.Equal(
            [
                "https://culinary.example/",
                "https://culinary.example/recipes",
                "https://culinary.example/categories",
                "https://culinary.example/categories/mon-chinh",
                "https://culinary.example/recipes/pho-bo"
            ],
            urls);
    }

    [Fact]
    public void GenerateAsync_RetriesTwiceThenFails()
    {
        var retry = typeof(SitemapGenerationJob)
            .GetMethod(nameof(SitemapGenerationJob.GenerateAsync))!
            .GetCustomAttributes(typeof(AutomaticRetryAttribute), inherit: true)
            .Cast<AutomaticRetryAttribute>()
            .Single();

        Assert.Equal(2, retry.Attempts);
        Assert.Equal(AttemptsExceededAction.Fail, retry.OnAttemptsExceeded);
    }
}
