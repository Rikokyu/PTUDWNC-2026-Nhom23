using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;

public sealed class GetRecipeByIdQueryHandler
    : IRequestHandler<GetRecipeByIdQuery, RecipeDetailDto>
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICurrentUser _currentUser;

    public GetRecipeByIdQueryHandler(
        IRecipeRepository recipeRepository,
        ICurrentUser currentUser)
    {
        _recipeRepository = recipeRepository;
        _currentUser = currentUser;
    }

    public async Task<RecipeDetailDto> Handle(
        GetRecipeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var recipe = await _recipeRepository.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException(
                $"Recipe with id '{request.Id}' was not found.");
        }

        if (recipe.Status != RecipeStatus.Published
            && !_currentUser.IsAdmin
            && (!_currentUser.UserId.HasValue
                || recipe.AuthorId != _currentUser.UserId))
        {
            throw new ForbiddenException(
                "You do not have permission to view this recipe.");
        }

        return RecipeDtoMapper.ToDetailDto(recipe);
    }
}