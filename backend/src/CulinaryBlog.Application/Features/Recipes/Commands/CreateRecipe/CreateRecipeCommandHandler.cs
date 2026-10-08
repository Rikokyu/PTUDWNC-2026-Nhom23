using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

public sealed class CreateRecipeCommandHandler
    : IRequestHandler<CreateRecipeCommand, RecipeSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateRecipeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<RecipeSummaryDto> Handle(
        CreateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        RecipeCommandHelper.EnsureAuthenticatedAuthor(_currentUser);
        RecipeCommandHelper.EnsureValidFields(
            request.Title,
            request.Description,
            request.Instructions,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Servings,
            request.Difficulty);

        var category = await _unitOfWork.Categories.GetActiveByIdReadOnlyAsync(
            request.CategoryId,
            cancellationToken);
        if (category is null)
        {
            throw new NotFoundException(
                $"Category with id '{request.CategoryId}' was not found.");
        }

        var title = request.Title!.Trim();
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = await RecipeCommandHelper.CreateUniqueSlugAsync(
                _unitOfWork.Recipes,
                title,
                excludingId: null,
                cancellationToken),
            Description = request.Description!.Trim(),
            Instructions = request.Instructions!.Trim(),
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            Difficulty = request.Difficulty,
            Status = RecipeStatus.Draft,
            CategoryId = category.Id,
            AuthorId = _currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Recipes.AddAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecipeCommandHelper.ToSummary(recipe, category);
    }
}