using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class RecipeSearchValidationTests
{
    // Invalid input must fail before attempting any PostgreSQL connection.
    private static SearchRecipesQueryHandler CreateHandler() => new(new RecipeRepository(
        new CulinaryBlogDbContext(new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=unused;Username=unused;Timeout=1")
            .Options)));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task InvalidFilters_AreRejectedBeforeDatabaseAccess(int kind)
    {
        var request = kind switch
        {
            0 => new SearchRecipesQuery("pho", 1, 10, CategoryId: Guid.Empty),
            1 => new SearchRecipesQuery("pho", 1, 10, Difficulty: (CulinaryBlog.Domain.Enums.DifficultyLevel)999),
            _ => new SearchRecipesQuery("pho", 1, 10, MaxCookTime: -1)
        };
        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(request, default));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("a")]
    [InlineData(" á ")]
    [InlineData("!!! | & :* ()")]
    public async Task InvalidTerm_IsRejectedBeforeDatabaseAccess(string term)
    {
        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(
            new SearchRecipesQuery(term, 1, 10), CancellationToken.None));
    }

    [Fact]
    public async Task ExcessivelyLongTerm_IsRejectedBeforeDatabaseAccess()
    {
        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(
            new SearchRecipesQuery(new string('a', 201), 1, 10), CancellationToken.None));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    [InlineData(int.MaxValue, 50)]
    public async Task InvalidPagination_IsRejectedBeforeDatabaseAccess(int page, int size)
    {
        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(
            new SearchRecipesQuery("phở", page, size), CancellationToken.None));
    }

    [Theory]
    [InlineData(" PHỞ  BÒ ", "phở:* & bò:*")]
    [InlineData("pho & bo:* | !", "pho:* & bo:*")]
    [InlineData("pho-pho", "pho:*")]
    [InlineData("phơ\u0309", "phở:*")]
    public void Terms_AreNormalizedAndUserOperatorsCannotChangeBooleanMeaning(string term, string expected)
    {
        Assert.Equal(expected, RecipeSearchText.BuildPrefixQuery(term));
    }

    [Theory]
    [InlineData("relevance", "relevance")]
    [InlineData("-relevance", "relevance")]
    [InlineData("TITLE", "title")]
    [InlineData("-createdAt", "-createdAt")]
    public void Sorts_AreCanonicalized(string value, string expected)
    {
        Assert.Equal(expected, SearchRecipesQuery.NormalizeSort(value));
    }

    [Fact]
    public async Task InvalidSort_IsRejectedBeforeDatabaseAccess()
    {
        await Assert.ThrowsAsync<ValidationException>(() => CreateHandler().Handle(
            new SearchRecipesQuery("phở", 1, 10, "unknown"), CancellationToken.None));
    }
}
