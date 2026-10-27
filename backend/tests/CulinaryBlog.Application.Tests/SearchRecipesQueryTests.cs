using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class SearchRecipesQueryTests
{
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    public async Task Handle_RejectsSearchTermsShorterThanTwoCharacters(
        string term)
    {
        var handler = new SearchRecipesQueryHandler(null!);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new SearchRecipesQuery(term, 1, 10),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RejectsPageSizeOutsideAllowedRange()
    {
        var handler = new SearchRecipesQueryHandler(null!);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new SearchRecipesQuery("soup", 1, 51),
                CancellationToken.None));
    }
}
