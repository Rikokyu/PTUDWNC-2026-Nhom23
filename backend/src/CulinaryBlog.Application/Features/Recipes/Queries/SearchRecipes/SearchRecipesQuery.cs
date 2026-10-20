using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;
using CulinaryBlog.Application.Common.Exceptions;

namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

public sealed record SearchRecipesQuery(
    string SearchTerm,
    int Page,
    int PageSize,
    string Sort = "relevance") : IRequest<PagedResult<RecipeSummaryDto>>
{
    private static readonly IReadOnlyDictionary<string, string> Sorts =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["relevance"] = "relevance",
            ["-relevance"] = "relevance",
            ["createdAt"] = "createdAt",
            ["-createdAt"] = "-createdAt",
            ["title"] = "title",
            ["-title"] = "-title",
            ["cookTime"] = "cookTime",
            ["-cookTime"] = "-cookTime",
            ["prepTime"] = "prepTime",
            ["-prepTime"] = "-prepTime",
            ["categoryName"] = "categoryName",
            ["-categoryName"] = "-categoryName"
        };

    public static string NormalizeSort(string? sort) =>
        string.IsNullOrWhiteSpace(sort) ? "relevance"
        : Sorts.TryGetValue(sort, out var canonical) ? canonical
        : throw new ValidationException("sort is invalid.");
}
