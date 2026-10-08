using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;

public sealed class GetRecipeBySlugQueryHandler
    : IRequestHandler<
        GetRecipeBySlugQuery,
        RecipeDetailDto>
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICurrentUser _currentUser;

    public GetRecipeBySlugQueryHandler(
        IRecipeRepository recipeRepository,
        ICurrentUser currentUser)
    {
        _recipeRepository = recipeRepository;
        _currentUser = currentUser;
    }

    public async Task<RecipeDetailDto> Handle(
        GetRecipeBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var recipe =
            await _recipeRepository
                .GetBySlugWithDetailsAsync(
                    request.Slug,
                    cancellationToken);

        if (recipe == null)
        {
            throw new NotFoundException(
                $"Recipe with slug '{request.Slug}' was not found.");
        }

        if (recipe.Status != CulinaryBlog.Domain.Enums.RecipeStatus.Published)
        {
            var isOwner =
                _currentUser.UserId.HasValue
                && recipe.AuthorId.HasValue
                && recipe.AuthorId.Value ==
                   _currentUser.UserId.Value;

            if (!_currentUser.IsAdmin && !isOwner)
            {
                throw new ForbiddenException(
                    "You do not have permission to view this recipe.");
            }
        }

        return RecipeDtoMapper.ToDetailDto(recipe);
    }
}