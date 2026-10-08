using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;

public sealed class DeleteRecipeCommandHandler
    : IRequestHandler<DeleteRecipeCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeleteRecipeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteRecipeCommand request,
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
        _unitOfWork.Recipes.Delete(recipe);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}