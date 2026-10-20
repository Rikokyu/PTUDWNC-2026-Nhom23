using System.Text.Json;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class RecipeSearchValidationHttpTests
{
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public async Task InvalidSearchTerm_IsReportedAsHttp422ProblemDetails(string term)
    {
        using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        await using var db = new CulinaryBlogDbContext(new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=unused;Username=unused;Timeout=1").Options);
        var handler = new SearchRecipesQueryHandler(new RecipeRepository(db));
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Path = "/api/v1/recipes/search";
        context.Request.Headers.Accept = "application/json";
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionMiddleware(
            async _ => { await handler.Handle(new(term, 1, 10), CancellationToken.None); },
            NullLogger<GlobalExceptionMiddleware>.Instance,
            services.GetRequiredService<IProblemDetailsService>());

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(422, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Contains("q must contain", problem.RootElement.GetProperty("detail").GetString());
    }
}
