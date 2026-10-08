using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;

public sealed record GetRecipeByIdQuery(Guid Id)
    : IRequest<RecipeDetailDto>;