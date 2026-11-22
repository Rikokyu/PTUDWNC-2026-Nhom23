using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using ConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Categories.Commands.BulkManageRecipes;

public sealed class BulkManageCategoryRecipesCommandHandler
    : IRequestHandler<BulkManageCategoryRecipesCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public BulkManageCategoryRecipesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(
        BulkManageCategoryRecipesCommand request,
        CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureAdmin(_currentUser);

        if (request.RecipeIds is null || request.RecipeIds.Count == 0)
            throw new ValidationException("Select at least one recipe.", "recipeIds");
        if (request.RecipeIds.Any(id => id == Guid.Empty)
            || request.RecipeIds.Distinct().Count() != request.RecipeIds.Count)
            throw new ValidationException("recipeIds must contain unique, valid recipe IDs.", "recipeIds");

        var source = await _unitOfWork.Categories.GetActiveByIdAsync(
            request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Source category was not found.");

        if (request.Action == BulkCategoryRecipeAction.Move)
        {
            if (!request.TargetCategoryId.HasValue
                || request.TargetCategoryId.Value == source.Id)
                throw new ValidationException("Select a different destination category.", "targetCategoryId");
            if (await _unitOfWork.Categories.GetActiveByIdAsync(
                    request.TargetCategoryId.Value, cancellationToken) is null)
                throw new NotFoundException("Destination category was not found.");
        }

        var recipes = new List<CulinaryBlog.Domain.Entities.Recipe>(request.RecipeIds.Count);
        foreach (var recipeId in request.RecipeIds)
        {
            var recipe = await _unitOfWork.Recipes.GetByIdWithDetailsAsync(
                recipeId, cancellationToken)
                ?? throw new NotFoundException($"Recipe '{recipeId}' was not found.");
            if (recipe.CategoryId != source.Id)
                throw new ConflictException("A selected recipe is no longer in this category. Refresh the list and try again.");
            recipes.Add(recipe);
        }

        foreach (var recipe in recipes)
        {
            if (request.Action == BulkCategoryRecipeAction.Move)
            {
                recipe.CategoryId = request.TargetCategoryId!.Value;
                recipe.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                recipe.SoftDelete();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return recipes.Count;
    }
}
