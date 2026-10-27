using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;

public sealed record GetRecipeBySlugQuery(
    string Slug
) : IRequest<RecipeDetailDto>;