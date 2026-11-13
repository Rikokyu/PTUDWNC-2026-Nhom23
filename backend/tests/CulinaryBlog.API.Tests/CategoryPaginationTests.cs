using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CulinaryBlog.API.Tests;

// Exercise real HTTP binding, endpoint validation and the production exception
// middleware. Only the query handler is replaced; no application database is used.
public sealed class CategoryPaginationTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddProblemDetails();
        builder.Services.AddAuthorization();
        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy("CategoryList", policy => policy.NoCache());
            options.AddPolicy("CategoryDetail", policy => policy.NoCache());
        });
        builder.Services.AddRateLimiter(options =>
            options.AddFixedWindowLimiter("Api", limiter =>
            {
                limiter.PermitLimit = 1000;
                limiter.Window = TimeSpan.FromMinutes(1);
            }));
        builder.Services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<CategoryPaginationTests>());
        _app = builder.Build();
        _app.UseMiddleware<GlobalExceptionMiddleware>();
        _app.UseRouting();
        _app.UseRateLimiter();
        _app.UseOutputCache();
        _app.MapCategoryEndpoints();
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    [Theory]
    [InlineData("", 1, 12)]
    [InlineData("?page=2", 2, 12)]
    [InlineData("?pageSize=5", 1, 5)]
    [InlineData("?page=3&pageSize=7", 3, 7)]
    [InlineData("?page=1&pageSize=50", 1, 50)]
    public async Task ValidPagination_ReachesHandlerWithExpectedValues(
        string query, int page, int pageSize)
    {
        using var response = await _client.GetAsync("/api/v1/categories/soup" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CategoryDetailDto>();
        Assert.NotNull(result);
        Assert.Equal(page, result.Recipes.Page);
        Assert.Equal(pageSize, result.Recipes.PageSize);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?page=-1")]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=-1")]
    [InlineData("?pageSize=51")]
    [InlineData("?page=abc")]
    [InlineData("?pageSize=abc")]
    [InlineData("?page=")]
    [InlineData("?pageSize=")]
    [InlineData("?page=2147483648")]
    [InlineData("?page=1&page=2")]
    public async Task InvalidPagination_ReturnsValidationProblem(string query)
    {
        using var response = await _client.GetAsync("/api/v1/categories/soup" + query);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?page=1&pageSize=12")]
    public async Task MissingCategory_ReturnsNotFound(string query)
    {
        using var response = await _client.GetAsync("/api/v1/categories/missing" + query);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    public sealed class StubCategoryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto>
    {
        public Task<CategoryDetailDto> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
        {
            if (request.Slug == "missing")
                throw new NotFoundException("Category not found.");
            return Task.FromResult(new CategoryDetailDto(
                new CategoryDto(Guid.Empty, "Soup", request.Slug, null, null, 0, 0),
                new PagedResult<RecipeSummaryDto>([], 0, request.Page, request.PageSize)));
        }
    }
}
