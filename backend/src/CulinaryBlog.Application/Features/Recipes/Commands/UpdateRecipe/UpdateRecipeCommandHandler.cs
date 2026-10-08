using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;

public sealed class UpdateRecipeCommandHandler
    : IRequestHandler<UpdateRecipeCommand, RecipeSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateRecipeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<RecipeSummaryDto> Handle(
        UpdateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await _unitOfWork.Recipes.GetByIdWithImagesAsync(
            request.Id,
            cancellationToken);
        if (recipe is null)
        {
            throw new NotFoundException(
                $"Recipe with id '{request.Id}' was not found.");
        }

        RecipeCommandHelper.EnsureCanManage(_currentUser, recipe);
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
        recipe.Title = title;
        recipe.Slug = await RecipeCommandHelper.CreateUniqueSlugAsync(
            _unitOfWork.Recipes,
            title,
            recipe.Id,
            cancellationToken);
        recipe.Description = request.Description!.Trim();
        recipe.Instructions = request.Instructions!.Trim();
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.Difficulty = request.Difficulty;
        recipe.CategoryId = category.Id;
        recipe.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Recipes.Update(recipe);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecipeCommandHelper.ToSummary(recipe, category);
    }
}