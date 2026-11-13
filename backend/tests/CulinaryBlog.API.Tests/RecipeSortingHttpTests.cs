using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using System.Net;
using Asp.Versioning;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class RecipeSortingHttpTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private StubHandler _handler = null!;
    private SearchStubHandler _search = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddProblemDetails();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
        });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<ICurrentUser, AnonymousUser>();
        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy("RecipeList", policy => policy.NoCache());
            options.AddPolicy("RecipeSearch", policy => policy.NoCache());
        });
        builder.Services.AddRateLimiter(options =>
            options.AddFixedWindowLimiter("Api", limiter =>
            {
                limiter.PermitLimit = 1000;
                limiter.Window = TimeSpan.FromMinutes(1);
            }));
        builder.Services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<RecipeSortingHttpTests>());
        _handler = new StubHandler();
        builder.Services.AddSingleton(_handler);
        builder.Services.AddTransient<IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>>(
            provider => provider.GetRequiredService<StubHandler>());
        _search = new SearchStubHandler();
        builder.Services.AddSingleton<IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>>(_search);
        _app = builder.Build();
        _app.UseMiddleware<GlobalExceptionMiddleware>();
        _app.UseRouting();
        _app.UseRateLimiter();
        _app.UseOutputCache();
        var versions = _app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build();
        _app.MapRecipeEndpoints(versions);
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    [Theory]
    [InlineData("title", "asc", "title")]
    [InlineData("title", "desc", "-title")]
    [InlineData("createdAt", "asc", "createdAt")]
    [InlineData("createdAt", "desc", "-createdAt")]
    [InlineData("cookTime", "asc", "cookTime")]
    [InlineData("cookTime", "desc", "-cookTime")]
    public async Task CanonicalSort_MapsToRepositorySort(
        string sortBy, string sortOrder, string expected)
    {
        using var response = await _client.GetAsync(
            $"/api/v1/recipes?sortBy={sortBy}&sortOrder={sortOrder}");
        Assert.True(response.StatusCode == HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        Assert.Equal(expected, _handler.Last?.Sort);
    }

    [Theory]
    [InlineData("?sortBy=invalid")]
    [InlineData("?sortOrder=invalid")]
    [InlineData("?sortBy=title&sortBy=cookTime")]
    [InlineData("?sortOrder=asc&sortOrder=desc")]
    public async Task InvalidOrDuplicateCanonicalSort_Returns422(string query)
    {
        using var response = await _client.GetAsync("/api/v1/recipes" + query);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Theory]
    [InlineData("", "-createdAt")]
    [InlineData("?sort=title", "title")]
    [InlineData("?sort=-cookTime", "-cookTime")]
    [InlineData("?sort=title&sortBy=cookTime&sortOrder=desc", "-cookTime")]
    public async Task DefaultLegacyAndCanonicalPrecedence(string query, string expected)
    {
        using var response = await _client.GetAsync("/api/v1/recipes" + query);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, _handler.Last?.Sort);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    [InlineData("1&minServings=2")]
    public async Task InvalidMinServings_Returns422(string value)
    {
        using var response = await _client.GetAsync("/api/v1/recipes?minServings=" + value);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CombinedFilters_AreForwarded()
    {
        var category = Guid.NewGuid();
        using var response = await _client.GetAsync(
            $"/api/v1/recipes?minServings=4&maxCookTime=30&categoryId={category}&difficulty=Easy&page=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, _handler.Last?.MinServings);
        Assert.Equal(30, _handler.Last?.MaxCookTime);
        Assert.Equal(category, _handler.Last?.CategoryId);
        Assert.Equal(2, _handler.Last?.Page);
    }

    [Theory]
    [InlineData("minServings=0")]
    [InlineData("minServings=1&minServings=2")]
    [InlineData("maxCookTime=-1")]
    [InlineData("maxCookTime=abc")]
    [InlineData("difficulty=999")]
    [InlineData("categoryId=abc")]
    public async Task InvalidSearchFilter_Returns422(string filter)
    {
        using var response = await _client.GetAsync("/api/v1/recipes/search?q=mi&" + filter);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task SearchFilters_AreForwarded()
    {
        var category = Guid.NewGuid();
        using var response = await _client.GetAsync(
            $"/api/v1/recipes/search?q=mi&categoryId={category}&difficulty=Easy&maxCookTime=10&minServings=4");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(category, _search.Last?.CategoryId);
        Assert.Equal(4, _search.Last?.MinServings);
        Assert.Equal(10, _search.Last?.MaxCookTime);
        Assert.Equal(CulinaryBlog.Domain.Enums.DifficultyLevel.Easy, _search.Last?.Difficulty);
    }

    public sealed class SearchStubHandler : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
    {
        public SearchRecipesQuery? Last { get; private set; }
        public Task<PagedResult<RecipeSummaryDto>> Handle(SearchRecipesQuery request, CancellationToken cancellationToken)
        {
            Last = request;
            return Task.FromResult(new PagedResult<RecipeSummaryDto>([], 0, request.Page, request.PageSize));
        }
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    public sealed class StubHandler : IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>
    {
        public GetRecipesQuery? Last { get; private set; }
        public Task<PagedResult<RecipeSummaryDto>> Handle(
            GetRecipesQuery request, CancellationToken cancellationToken)
        {
            Last = request;
            return Task.FromResult(new PagedResult<RecipeSummaryDto>([], 0,
                request.Page, request.PageSize));
        }
    }

    private sealed class AnonymousUser : ICurrentUser
    {
        public Guid? UserId => null;
        public bool IsAuthenticated => false;
        public bool IsAuthor => false;
        public bool IsAdmin => false;
    }
}
